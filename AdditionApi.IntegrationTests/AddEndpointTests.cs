using System.Net.Http.Json;
using Xunit;

namespace AdditionApi.IntegrationTests;

public class AddEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AddEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(Skip = "Skipped because of VS testhost '--parentprocessid' bug on this machine.")]
    public async Task Add_ReturnsExpectedResult()
    {
        var request = new { a = 2, b = 3 };

        var response = await _client.PostAsJsonAsync("/add", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<int>();

        Assert.Equal(5, result);
    }
}
