using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using sqlapi_integrationtest;
using Xunit;

namespace sqlapi_integrationtest;

public class SqlTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;


    private readonly HttpClient client;

    public SqlTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        client = _fixture.CreateClient();
    }

    // Test interger should be valid
    [Theory]
    [InlineData(100)]
    [InlineData(-50)]
    [InlineData(0)]
    public async Task GivenInteger_WhenPosting_ThenItShouldBeSaved(int validNumber)
    {
        // Arrange
        var client = _fixture.WebApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/number", new NumberInput(validNumber));

        // Assert
        response.EnsureSuccessStatusCode();
        
        var getResponse = await client.GetFromJsonAsync<List<int>>("/number");
        Assert.NotNull(getResponse);
        Assert.Contains(validNumber, getResponse);
    }

    // negative, should reject decimals
    [Fact]
    public async Task GivenDecimal_WhenPosting_ThenReturnBadRequest()
    {
        // Arrange
        var client = _fixture.WebApplicationFactory.CreateClient();
        
        var invalidInput = new { Value = 12.5 }; 

        // Act
        var response = await client.PostAsJsonAsync("/number", invalidInput);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // negative, should reject strings
    [Fact]
    public async Task GivenString_WhenPosting_ThenReturnBadRequest()
    {
        // Arrange
        var client = _fixture.WebApplicationFactory.CreateClient();
        
        var invalidInput = new { Value = "not-a-number" };

        // Act
        var response = await client.PostAsJsonAsync("/number", invalidInput);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact] // test get number, should ensure number is saved
    public async Task GivenStoredNumber_WhenGetting_ThenReturnsTheNumber()
    {
        // 1. Arrange 
        int testNumber = 888;
        var postResponse = await client.PostAsJsonAsync("/number", new { Value = testNumber });
        postResponse.EnsureSuccessStatusCode();

        // 2. Act
        var getResponse = await client.GetFromJsonAsync<List<int>>("/number");

        // 3. Assert
        Assert.NotNull(getResponse); 

        Assert.Contains(testNumber, getResponse);
    }
    internal record NumberInput(int Value);
}


