using System.Net;
using System.Net.Http.Json;
using System.Text;
using AdditionApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdditionApi.IntegrationTests;

[Collection("Api")]
public class StorageEndpointTests
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public StorageEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Each test uses its own key so tests don't depend on each other's data.
    private static string NewKey() => $"key-{Guid.NewGuid():N}";

    private record StorageValue(string? Value);

    // ---------- Positive tests ----------

    [Fact]
    public async Task Put_NewKey_ReturnsNoContent()
    {
        var response = await _client.PutAsJsonAsync($"/storage/{NewKey()}", new StorageValue("42"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Put_ThenGet_ReturnsStoredValue()
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("42"));

        var response = await _client.GetAsync($"/storage/{key}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<StorageValue>();
        Assert.Equal("42", body?.Value);
    }

    [Fact]
    public async Task Put_ExistingKey_OverwritesValue()
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("first"));

        var response = await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("second"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var body = await _client.GetFromJsonAsync<StorageValue>($"/storage/{key}");
        Assert.Equal("second", body?.Value);
    }

    [Fact]
    public async Task Put_PersistsValueInDatabase()
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("persisted"));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StorageContext>();
        var entry = await db.StorageEntries.AsNoTracking().SingleOrDefaultAsync(e => e.Key == key);

        Assert.NotNull(entry);
        Assert.Equal("persisted", entry.Value);
    }

    [Fact]
    public async Task Put_ExistingKey_DoesNotCreateDuplicateRow()
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("a"));
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("b"));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StorageContext>();

        Assert.Equal(1, await db.StorageEntries.CountAsync(e => e.Key == key));
    }

    [Fact]
    public async Task Put_DifferentKeys_AreStoredIndependently()
    {
        var keyA = NewKey();
        var keyB = NewKey();
        await _client.PutAsJsonAsync($"/storage/{keyA}", new StorageValue("A"));
        await _client.PutAsJsonAsync($"/storage/{keyB}", new StorageValue("B"));

        Assert.Equal("A", (await _client.GetFromJsonAsync<StorageValue>($"/storage/{keyA}"))?.Value);
        Assert.Equal("B", (await _client.GetFromJsonAsync<StorageValue>($"/storage/{keyB}"))?.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello world")]
    [InlineData("åäö ÅÄÖ 😀")]
    public async Task Put_ThenGet_RoundTripsSpecialValues(string value)
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue(value));

        var body = await _client.GetFromJsonAsync<StorageValue>($"/storage/{key}");

        Assert.Equal(value, body?.Value);
    }

    // ---------- Negative tests ----------

    [Fact]
    public async Task Get_UnknownKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/storage/{NewKey()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_SimilarButDifferentKey_ReturnsNotFound()
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("x"));

        var response = await _client.GetAsync($"/storage/{key}-other");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithoutBody_ReturnsBadRequest()
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/storage/{NewKey()}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithMalformedJson_ReturnsBadRequest()
    {
        var content = new StringContent("{ \"value\": ", Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/storage/{NewKey()}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithNullValue_ReturnsBadRequestAndStoresNothing()
    {
        var key = NewKey();

        var response = await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue(null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/storage/{key}")).StatusCode);
    }

    [Fact]
    public async Task Put_WithMissingValueProperty_ReturnsBadRequest()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/storage/{NewKey()}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithWrongValueType_ReturnsBadRequest()
    {
        var content = new StringContent("{ \"value\": 42 }", Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/storage/{NewKey()}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithNonJsonContentType_ReturnsUnsupportedMediaType()
    {
        var content = new StringContent("42", Encoding.UTF8, "text/plain");

        var response = await _client.PutAsync($"/storage/{NewKey()}", content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithTooLongKey_ReturnsBadRequest()
    {
        var key = new string('k', 451);

        var response = await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("x"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_FailedRequest_DoesNotOverwriteExistingValue()
    {
        var key = NewKey();
        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue("original"));

        await _client.PutAsJsonAsync($"/storage/{key}", new StorageValue(null));

        var body = await _client.GetFromJsonAsync<StorageValue>($"/storage/{key}");
        Assert.Equal("original", body?.Value);
    }

    [Fact]
    public async Task Delete_IsNotSupported_ReturnsMethodNotAllowed()
    {
        var response = await _client.DeleteAsync($"/storage/{NewKey()}");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithoutKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/storage/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
