using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace AdditionApi.Tests;

public class AdditionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AdditionApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        Database.Clear();
    }

    [Fact]
    public async Task Post_NewKey_ReturnsCreated()
    {
        var payload = new { Key = "testKey", Value = "123" };

        var response = await _client.PostAsJsonAsync("/api/addition", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_DuplicateKey_ReturnsBadRequest()
    {
        var payload = new { Key = "duplicate", Value = "1" };
        
        await _client.PostAsJsonAsync("/api/addition", payload);

        var secondResponse = await _client.PostAsJsonAsync("/api/addition", payload);

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Get_ExistingKey_ReturnsCorrectValue()
    {
        var key = "findMe";
        await _client.PostAsJsonAsync("/api/addition", new { Key = key, Value = "found" });

        var response = await _client.GetAsync($"/api/addition/{key}");
        var data = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("found", data?["value"]);
    }
    [Fact]
    public async Task Get_NonExistentKey_ReturnsNotFound()
    {
        Database.Clear();
        var randomKey = "thisKeyDoesNotExist_" + Guid.NewGuid();

        var response = await _client.GetAsync($"/api/addition/{randomKey}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}