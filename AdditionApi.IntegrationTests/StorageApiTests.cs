using AdditionApi;
using System.Net;
using System.Net.Http.Json;

namespace AdditionApi.IntegrationTests;

public class StorageApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public StorageApiTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        await _factory.StartDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.StopDatabaseAsync();
    }

    [Fact]
    public async Task PostStorageItem_ReturnsOk()
    {
        var item = new
        {
            key = "name",
            value = "Tinju"
        };

        var response = await _client.PostAsJsonAsync("/storage", item);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStorageItem_ReturnsStoredItem()
    {
        var item = new
        {
            key = "name",
            value = "Tinju"
        };

        await _client.PostAsJsonAsync("/storage", item);

        var response = await _client.GetAsync("/storage/name");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StorageItem>();

        Assert.NotNull(result);
        Assert.Equal("name", result.Key);
        Assert.Equal("Tinju", result.Value);
    }

    [Fact]
    public async Task GetStorageItem_WhenKeyDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/storage/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostStorageItem_WhenKeyAlreadyExists_UpdatesValue()
    {
        var firstItem = new
        {
            key = "name",
            value = "Tinju"
        };

        var secondItem = new
        {
            key = "name",
            value = "Joseph"
        };

        await _client.PostAsJsonAsync("/storage", firstItem);
        await _client.PostAsJsonAsync("/storage", secondItem);

        var response = await _client.GetAsync("/storage/name");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<StorageItem>();

        Assert.NotNull(result);
        Assert.Equal("name", result.Key);
        Assert.Equal("Joseph", result.Value);
    }
}
