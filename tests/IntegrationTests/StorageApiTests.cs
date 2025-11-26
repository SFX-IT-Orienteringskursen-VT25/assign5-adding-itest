using System.Net.Http.Json;
using AdditionApi.Models;

namespace IntegrationTests;

[Collection("IntegrationTests collection")]
public class StorageApiTests
{
    private readonly HttpClient _client;

    public StorageApiTests(SqlTestContainerFixture sqlFixture)
    {
        var factory = new CustomWebApplicationFactory(sqlFixture.Container);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SetItem_ShouldStoreValue()
    {
        var item = new StorageItem { Key = "test1", Value = "hello" };

        var response = await _client.PostAsJsonAsync("/storage", item);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetItem_ShouldReturnStoredValue()
    {
        var item = new StorageItem { Key = "test2", Value = "world" };
        await _client.PostAsJsonAsync("/storage", item);

        var result = await _client.GetFromJsonAsync<StorageItem>("/storage/test2");

        Assert.Equal("world", result!.Value);
    }

    [Fact]
    public async Task GetItem_ShouldReturnNotFound_ForMissingKey()
    {
        var response = await _client.GetAsync("/storage/unknown_key");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
