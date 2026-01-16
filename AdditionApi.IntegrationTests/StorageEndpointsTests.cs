using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdditionApi.IntegrationTests;

public class StorageEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public StorageEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_ReturnsHelloWorld()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello World!", content);
    }

    private async Task ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.StorageItems.RemoveRange(context.StorageItems);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_EnteredNumbers_ReturnsEmpty_WhenNoData()
    {
        // Arrange
        await ClearDatabase();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/storage/enteredNumbers");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<NumbersResponse>();
        Assert.NotNull(result);
        Assert.Empty(result.numbers);
    }

    [Fact]
    public async Task Put_EnteredNumbers_StoresAndReturnsNumbers()
    {
        // Arrange
        var client = _factory.CreateClient();
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var request = new NumbersRequest(numbers);

        // Act
        var response = await client.PutAsJsonAsync("/storage/enteredNumbers", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<NumbersResponse>();
        Assert.NotNull(result);
        Assert.Equal(numbers, result.numbers);

        // Verify persistence
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storedItem = await context.StorageItems.FirstOrDefaultAsync(x => x.Key == "enteredNumbers");
        Assert.NotNull(storedItem);
        Assert.Contains("[1,2,3,4,5]", storedItem.Value);
    }
    
    [Fact]
    public async Task Put_EnteredNumbers_UpdatesExistingData()
    {
        // Arrange
        var client = _factory.CreateClient();
        var initialNumbers = new[] { 1, 2 };
        await client.PutAsJsonAsync("/storage/enteredNumbers", new NumbersRequest(initialNumbers));
        
        var newNumbers = new[] { 3, 4, 5 };

        // Act
        var response = await client.PutAsJsonAsync("/storage/enteredNumbers", new NumbersRequest(newNumbers));

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<NumbersResponse>();
        Assert.Equal(newNumbers, result.numbers);
    }

    [Fact]
    public async Task Put_EnteredNumbers_ReturnsBadRequest_WhenInvalidData()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act & Assert
        // Sending invalid JSON to trigger 400 Bad Request
        var response = await client.PostAsync("/storage/enteredNumbers", new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json"));
        
        // The endpoint is mapped as PUT, so POST should return 405 Method Not Allowed
        Assert.Equal(System.Net.HttpStatusCode.MethodNotAllowed, response.StatusCode);

        // Let's try a real negative test for the PUT endpoint
        // If we send a request with a body that cannot be deserialized to NumbersRequest
        var responseBadRequest = await client.PutAsync("/storage/enteredNumbers", new StringContent("{ \"Numbers\": \"invalid\" }", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, responseBadRequest.StatusCode);
    }
}

public record NumbersResponse(int[] numbers);
