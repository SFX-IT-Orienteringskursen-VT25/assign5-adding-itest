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
        // Act
        var response = await _client.GetAsync("/api/Addition/NumberList");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTotal_ReturnsOkResult()
    {
        // Act
        var response = await _client.GetAsync("/api/Addition/TotalSum");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
