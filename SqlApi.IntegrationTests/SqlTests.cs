using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SetupMssqlExample;
using Xunit;

namespace SetupMssqlExample;

public class SqlTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public SqlTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
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

    internal record NumberInput(int Value);
}