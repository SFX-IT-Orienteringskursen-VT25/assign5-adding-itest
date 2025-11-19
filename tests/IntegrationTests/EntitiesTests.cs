// tests/IntegrationTests/EntitiesTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests;

[Collection("IntegrationTests collection")]
public class EntitiesTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EntitiesTests(SqlTestContainerFixture sqlFixture)
    {
        // фабрика должна получать ссылку на фикстуру
        _factory = new CustomWebApplicationFactory(sqlFixture);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetRootEndpoint_ShouldReturnOk()
    {
        var resp = await _client.GetAsync("/");
        resp.EnsureSuccessStatusCode();
        var content = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Hello", content); // или точная проверка
    }

    [Fact]
    public async Task AddEndpoint_ShouldReturnCorrectSum()
    {
        var resp = await _client.GetAsync("/add?a=2&b=3");
        resp.EnsureSuccessStatusCode();
        var sum = int.Parse(await resp.Content.ReadAsStringAsync());
        Assert.Equal(5, sum);
    }

    // Тесты для твоего StorageController (пример)
    [Fact]
    public async Task Storage_Post_Get_ShouldReturnSavedValue()
    {
        var item = new { Key = "k1", Value = "v1" };
        var post = await _client.PostAsJsonAsync("/storage", item);
        post.EnsureSuccessStatusCode();

        var get = await _client.GetAsync($"/storage/{item.Key}");
        get.EnsureSuccessStatusCode();
        var stored = await get.Content.ReadFromJsonAsync<StorageItemDto>()!;
        Assert.Equal(item.Value, stored.Value);
    }

    [Fact]
    public async Task Storage_Get_NotFound_ForUnknownKey()
    {
        var get = await _client.GetAsync($"/storage/non-existing-key");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    // DTO для парсинга результата (создай внутри файла или вынеси)
    private record StorageItemDto(string Key, string Value);
}
