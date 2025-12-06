using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace AdditionApi.IntegrationTest;

public class AdditionApiTest(SqlContainerFactory sqlContainerFactory): IClassFixture<SqlContainerFactory>
{
    private readonly HttpClient _client = new CustomWebApplicationFactory(sqlContainerFactory.SqlContainer).CreateClient();
    
    // GET / Tests
    [Fact]
    public async Task Get_DefaultEndpoint_ShouldReturnApiName()
    {
        // Act
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("assign3-addition-api", content);
    }

    // POST /api/storage Tests - Positive Cases
    [Fact]
    public async Task PostStorage_WithValidRecord_ShouldCreateRecord()
    {
        // Arrange
        var record = new { Key = "test-key", Value = "test-value" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/storage", record);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/storage/test-key", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("key1", "value1")]
    [InlineData("key-with-hyphens", "value-with-hyphens")]
    [InlineData("KeyWithNumbers123", "ValueWithNumbers456")]
    public async Task GetStorage_WithVariousExistingKeys_ShouldReturnCorrectValues(string key, string value)
    {
        // Arrange
        var record = new Record { Key = key, Value = value };
        await _client.PostAsJsonAsync("/api/storage", record);

        // Act
        var response = await _client.GetFromJsonAsync<Record?>($"/api/storage/{key}");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(value, response.Value);
    }

    // GET /api/storage/{key} Tests - Error Cases
    [Fact]
    public async Task GetStorage_WithNonExistentKey_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/storage/non-existent-key-12345");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    // POST /api/storage Tests - Validation Cases
    [Fact]
    public async Task PostStorage_WithEmptyKey_ShouldReturnBadRequest()
    {
        // Arrange
        var record = new Record { Key = "", Value = "test-value" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/storage", record);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Integration Workflow Tests
    [Fact]
    public async Task CompleteWorkflow_CreateThenRetrieve_ShouldWork()
    {
        // Arrange
        var key = $"workflow-test-{Guid.NewGuid()}";
        var value = "test workflow value with special chars: åäö 123!@#";
        var record = new Record { Key = key, Value = value };

        // Act - Create record
        var createResponse = await _client.PostAsJsonAsync("/api/storage", record);
        
        // Assert - Verify creation
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal($"/api/storage/{key}", createResponse.Headers.Location?.ToString());

        // Act - Retrieve record
        var retrieveResponse = await _client.GetFromJsonAsync<Record?>($"/api/storage/{key}");

        // Assert - Verify retrieval
        Assert.NotNull(retrieveResponse);
        Assert.Equal(key, retrieveResponse.Key);
        Assert.Equal(value, retrieveResponse.Value);
    }

}