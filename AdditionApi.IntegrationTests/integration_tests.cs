using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using SetupMssqlExample;
using Xunit;

public class LocalStorageEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LocalStorageEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // POSITIVE TESTS
    
    [Fact]
    public async Task SetItem_Then_GetItem_Returns_Value()
    {
        var putResponse = await _client.PutAsJsonAsync("/localStorage/setItem/mykey", new { value = "abc" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync("/localStorage/getItem/mykey");
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadFromJsonAsync<ValueDto>();
        body!.value.Should().Be("abc");
    }

    [Fact]
    public async Task SetItem_Then_UpdateItem_Overwrites_Value()
    {
        // Set initial value
        await _client.PutAsJsonAsync("/localStorage/setItem/updatekey", new { value = "original" });
        
        // Update with new value
        await _client.PutAsJsonAsync("/localStorage/setItem/updatekey", new { value = "updated" });
        
        // Verify it's updated
        var getResponse = await _client.GetAsync("/localStorage/getItem/updatekey");
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadFromJsonAsync<ValueDto>();
        body!.value.Should().Be("updated");
    }

    [Fact]
    public async Task SetItem_With_Null_Value_Stores_Null()
    {
        await _client.PutAsJsonAsync("/localStorage/setItem/nullkey", new { value = (string?)null });
        
        var getResponse = await _client.GetAsync("/localStorage/getItem/nullkey");
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadFromJsonAsync<ValueDto>();
        body!.value.Should().BeNull();
    }

    [Fact]
    public async Task SetItem_With_Empty_String_Stores_Empty()
    {
        await _client.PutAsJsonAsync("/localStorage/setItem/emptykey", new { value = "" });
        
        var getResponse = await _client.GetAsync("/localStorage/getItem/emptykey");
        getResponse.EnsureSuccessStatusCode();
        var body = await _client.GetStringAsync("/localStorage/getItem/emptykey");
        body.Should().Contain("\"value\":\"\"");
    }

    [Fact]
    public async Task Multiple_Keys_Work_Independently()
    {
        await _client.PutAsJsonAsync("/localStorage/setItem/key1", new { value = "value1" });
        await _client.PutAsJsonAsync("/localStorage/setItem/key2", new { value = "value2" });
        
        var response1 = await _client.GetAsync("/localStorage/getItem/key1");
        var body1 = await response1.Content.ReadFromJsonAsync<ValueDto>();
        body1!.value.Should().Be("value1");
        
        var response2 = await _client.GetAsync("/localStorage/getItem/key2");
        var body2 = await response2.Content.ReadFromJsonAsync<ValueDto>();
        body2!.value.Should().Be("value2");
    }

    [Fact]
    public async Task Root_Endpoint_Returns_HelloWorld()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Hello World!");
    }

    // NEGATIVE TESTS

    [Fact]
    public async Task GetItem_For_Missing_Key_Returns_Null()
    {
        var response = await _client.GetAsync("/localStorage/getItem/unknown");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ValueDto>();
        body!.value.Should().BeNull();
    }

    [Fact]
    public async Task SetItem_Without_Body_Returns_BadRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/localStorage/setItem/nobody")
        {
            Content = null
        };

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetItem_With_Invalid_JSON_Returns_BadRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/localStorage/setItem/invalid")
        {
            Content = new StringContent("not valid json", Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetItem_With_Very_Long_Value_Works()
    {
        var longValue = new string('a', 10000); // 10,000 character string
        await _client.PutAsJsonAsync("/localStorage/setItem/longkey", new { value = longValue });
        
        var getResponse = await _client.GetAsync("/localStorage/getItem/longkey");
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadFromJsonAsync<ValueDto>();
        body!.value.Should().Be(longValue);
    }

    [Fact]
    public async Task SetItem_With_Special_Characters_In_Key_Works()
    {
        var specialKey = "key-with-special-chars-123";
        await _client.PutAsJsonAsync($"/localStorage/setItem/{specialKey}", new { value = "test" });
        
        var getResponse = await _client.GetAsync($"/localStorage/getItem/{specialKey}");
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadFromJsonAsync<ValueDto>();
        body!.value.Should().Be("test");
    }

    private sealed class ValueDto
    {
        public string? value { get; set; }
    }
}