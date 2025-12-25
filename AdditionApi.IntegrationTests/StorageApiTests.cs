using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AdditionApi.IntegrationTests;

public class StorageApiTests : IClassFixture<AdditionApiFactory>
{
    private readonly HttpClient _client;
    private readonly AdditionApiFactory _factory;

    public StorageApiTests(AdditionApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region PUT /storage/{key} Tests

    [Fact]
    public async Task Put_NewKey_Returns201Created()
    {
        // Arrange
        var key = "testKey1";
        var value = "testValue1";
        var payload = new { Value = value };

        // Act
        var response = await _client.PutAsJsonAsync($"/storage/{key}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(value, content.Value);
        Assert.Equal($"/storage/{key}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Put_ExistingKey_Returns200Ok()
    {
        // Arrange
        var key = "testKey2";
        var initialValue = "initialValue";
        var updatedValue = "updatedValue";
        var initialPayload = new { Value = initialValue };
        var updatedPayload = new { Value = updatedValue };

        // First, create the key
        await _client.PutAsJsonAsync($"/storage/{key}", initialPayload);

        // Act - Update the same key
        var response = await _client.PutAsJsonAsync($"/storage/{key}", updatedPayload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(updatedValue, content.Value);
    }

    [Fact]
    public async Task Put_EmptyValue_Returns200Or201()
    {
        // Arrange
        var key = "emptyValueKey";
        var payload = new { Value = "" };

        // Act
        var response = await _client.PutAsJsonAsync($"/storage/{key}", payload);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                    response.StatusCode == HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal("", content.Value);
    }

    [Fact]
    public async Task Put_SpecialCharactersInKey_ReturnsSuccess()
    {
        // Arrange
        var key = "key-with_special.chars";
        var value = "testValue";
        var payload = new { Value = value };

        // Act
        var response = await _client.PutAsJsonAsync($"/storage/{key}", payload);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(value, content.Value);
    }

    [Fact]
    public async Task Put_LongValue_ReturnsSuccess()
    {
        // Arrange
        var key = "longValueKey";
        var value = new string('a', 1000); // 1000 character string
        var payload = new { Value = value };

        // Act
        var response = await _client.PutAsJsonAsync($"/storage/{key}", payload);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(value, content.Value);
    }

    [Fact]
    public async Task Put_WithoutBody_Returns400BadRequest()
    {
        // Arrange
        var key = "testKey";

        // Act
        var response = await _client.PutAsync($"/storage/{key}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_InvalidJson_Returns400BadRequest()
    {
        // Arrange
        var key = "testKey";
        var invalidJson = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/storage/{key}", invalidJson);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GET /storage/{key} Tests

    [Fact]
    public async Task Get_ExistingKey_Returns200WithValue()
    {
        // Arrange
        var key = "getTestKey1";
        var value = "getTestValue1";
        var payload = new { Value = value };
        await _client.PutAsJsonAsync($"/storage/{key}", payload);

        // Act
        var response = await _client.GetAsync($"/storage/{key}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(value, content.Value);
    }

    [Fact]
    public async Task Get_NonExistentKey_Returns404NotFound()
    {
        // Arrange
        var key = "nonExistentKey";

        // Act
        var response = await _client.GetAsync($"/storage/{key}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(content);
        Assert.Contains(key, content.Message);
    }

    [Fact]
    public async Task Get_EmptyKey_Returns404NotFound()
    {
        // Arrange - empty key will result in a different route

        // Act
        var response = await _client.GetAsync("/storage/");

        // Assert
        // This should return 404 because the route doesn't match
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_AfterUpdate_ReturnsUpdatedValue()
    {
        // Arrange
        var key = "updateTestKey";
        var initialValue = "initialValue";
        var updatedValue = "updatedValue";
        
        await _client.PutAsJsonAsync($"/storage/{key}", new { Value = initialValue });
        await _client.PutAsJsonAsync($"/storage/{key}", new { Value = updatedValue });

        // Act
        var response = await _client.GetAsync($"/storage/{key}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(updatedValue, content.Value);
    }

    [Fact]
    public async Task Get_SpecialCharactersInKey_ReturnsCorrectValue()
    {
        // Arrange
        var key = "key-with_special.chars2";
        var value = "specialValue";
        await _client.PutAsJsonAsync($"/storage/{key}", new { Value = value });

        // Act
        var response = await _client.GetAsync($"/storage/{key}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.NotNull(content);
        Assert.Equal(key, content.Key);
        Assert.Equal(value, content.Value);
    }

    #endregion

    #region Integration Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateReadUpdateRead_WorksCorrectly()
    {
        // Arrange
        var key = "workflowKey";
        var initialValue = "initialValue";
        var updatedValue = "updatedValue";

        // Act & Assert - Create
        var createResponse = await _client.PutAsJsonAsync($"/storage/{key}", new { Value = initialValue });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Act & Assert - Read initial value
        var readResponse1 = await _client.GetAsync($"/storage/{key}");
        Assert.Equal(HttpStatusCode.OK, readResponse1.StatusCode);
        var content1 = await readResponse1.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.Equal(initialValue, content1!.Value);

        // Act & Assert - Update
        var updateResponse = await _client.PutAsJsonAsync($"/storage/{key}", new { Value = updatedValue });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Act & Assert - Read updated value
        var readResponse2 = await _client.GetAsync($"/storage/{key}");
        Assert.Equal(HttpStatusCode.OK, readResponse2.StatusCode);
        var content2 = await readResponse2.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.Equal(updatedValue, content2!.Value);
    }

    [Fact]
    public async Task MultipleKeys_CanBeStoredIndependently()
    {
        // Arrange
        var key1 = "multiKey1";
        var key2 = "multiKey2";
        var key3 = "multiKey3";
        var value1 = "value1";
        var value2 = "value2";
        var value3 = "value3";

        // Act - Store multiple keys
        await _client.PutAsJsonAsync($"/storage/{key1}", new { Value = value1 });
        await _client.PutAsJsonAsync($"/storage/{key2}", new { Value = value2 });
        await _client.PutAsJsonAsync($"/storage/{key3}", new { Value = value3 });

        // Assert - Retrieve and verify each key independently
        var response1 = await _client.GetAsync($"/storage/{key1}");
        var content1 = await response1.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.Equal(value1, content1!.Value);

        var response2 = await _client.GetAsync($"/storage/{key2}");
        var content2 = await response2.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.Equal(value2, content2!.Value);

        var response3 = await _client.GetAsync($"/storage/{key3}");
        var content3 = await response3.Content.ReadFromJsonAsync<StorageResponse>();
        Assert.Equal(value3, content3!.Value);
    }

    #endregion

    #region Negative Tests

    [Fact]
    public async Task Get_WithInvalidMethod_Returns405MethodNotAllowed()
    {
        // Arrange
        var key = "testKey";

        // Act
        var response = await _client.PostAsync($"/storage/{key}", null);

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Put_MissingValueProperty_Returns400BadRequest()
    {
        // Arrange
        var key = "testKey";
        var payload = new { WrongProperty = "someValue" };

        // Act
        var response = await _client.PutAsJsonAsync($"/storage/{key}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}

// Response models for deserialization
public record StorageResponse(string Key, string Value);
public record ErrorResponse(string Message);
