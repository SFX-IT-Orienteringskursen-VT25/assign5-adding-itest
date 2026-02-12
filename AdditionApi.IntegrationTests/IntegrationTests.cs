using System.Net;
using System.Net.Http.Json;

namespace AdditionApi.IntegrationTests;

public class AdditionControllerIntegrationTests : IClassFixture<AdditionApiFactory>
{
    private readonly HttpClient _client;

    public AdditionControllerIntegrationTests(AdditionApiFactory factory)
    {
        _client = factory.WebApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task GetNumbers_ReturnsOkResult()
    {
        // Arrange - Clear database and insert known values
        await _client.DeleteAsync("/api/Addition/DeleteAll");
        await _client.PostAsJsonAsync("/api/Addition", "10");
        await _client.PostAsJsonAsync("/api/Addition", "20");
        await _client.PostAsJsonAsync("/api/Addition", "30");

        // Act
        var response = await _client.GetAsync("/api/Addition/NumberList");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var numbers = await response.Content.ReadFromJsonAsync<List<int>>();
        Assert.NotNull(numbers);
        Assert.Equal(3, numbers.Count);
        Assert.Contains(10, numbers);
        Assert.Contains(20, numbers);
        Assert.Contains(30, numbers);
    }

    [Fact]
    public async Task GetTotal_ReturnsOkResult()
    {
        // Arrange - Clear database and insert known values
        await _client.DeleteAsync("/api/Addition/DeleteAll");
        await _client.PostAsJsonAsync("/api/Addition", "10");
        await _client.PostAsJsonAsync("/api/Addition", "20");
        await _client.PostAsJsonAsync("/api/Addition", "30");

        // Act
        var response = await _client.GetAsync("/api/Addition/TotalSum");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var total = await response.Content.ReadFromJsonAsync<int>();
        Assert.Equal(60, total);
    }

    [Fact]
    public async Task InsertNumber_WithValidInteger_ReturnsOk()
    {
        // Arrange
        var numberToInsert = "42";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Addition", numberToInsert);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InsertNumber_WithInvalidValue_ReturnsBadRequest()
    {
        // Arrange
        var invalidValue = "not-a-number";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Addition", invalidValue);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAllNumbers_ReturnsOk()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Addition/DeleteAll");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
