using System.Net;
using FluentAssertions;
using Xunit;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace AdditionApi.IntegrationTests;

public class AdditionApiTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture fixture;

    public AdditionApiTests(IntegrationTestFixture fixture)
    {
        this.fixture = fixture;
    }

    public class SavedNumbersResponse
{
    public List<int> savedNumbers { get; set; } = new();
}

    [Fact]
    public async Task GetRoot_ReturnsHelloWorld()
    {
        // Arrange
        fixture.GetDatabase().DeleteAll();
        var client = fixture.WebApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Hello World!");
    }

    [Fact]
public async Task GetNumbers_WhenEmpty_ReturnsEmptyArray()
{
    // Arrange
    fixture.GetDatabase().DeleteAll();
    var client = fixture.WebApplicationFactory.CreateClient();

    // Act
    var response = await client.GetAsync("/numbers");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var result = await response.Content.ReadFromJsonAsync<SavedNumbersResponse>();
    result.Should().NotBeNull();
    result!.savedNumbers.Should().BeEmpty();
}

[Fact]
public async Task GetNumbers_WhenNotEmpty_ReturnsSavedNumbers()
{
    // Arrange
    fixture.GetDatabase().DeleteAll();
    var db = fixture.GetDatabase();
    db.InsertValue(3);
    db.InsertValue(7);

    var client = fixture.WebApplicationFactory.CreateClient();

    // Act
    var response = await client.GetAsync("/numbers");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var result = await response.Content.ReadFromJsonAsync<SavedNumbersResponse>();
    result.Should().NotBeNull();
    result!.savedNumbers.Should().HaveCount(2);
    result.savedNumbers.Should().Contain(new[] { 3, 7 });
}

[Fact]
public async Task PostNumbers_AddsNumber_ReturnsUpdatedList()
{
    // Arrange
    fixture.GetDatabase().DeleteAll();
    var client = fixture.WebApplicationFactory.CreateClient();

    var request = new { number = 5 };

    // Act
    var response = await client.PostAsJsonAsync("/numbers", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var result = await response.Content.ReadFromJsonAsync<SavedNumbersResponse>();
    result.Should().NotBeNull();
    result!.savedNumbers.Should().ContainSingle();
    result.savedNumbers.Should().Contain(5);
}

[Fact]
public async Task PostNumbers_EmptyBody_ReturnsBadRequest()
{
    // Arrange
    var client = fixture.WebApplicationFactory.CreateClient();
    var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

    // Act
    var response = await client.PostAsync("/numbers", content);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
}
