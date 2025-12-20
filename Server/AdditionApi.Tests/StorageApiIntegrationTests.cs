 using Microsoft.EntityFrameworkCore;
 using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;
using AdditionApi.Data;

public class StorageApiIntegrationTests 
    : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public StorageApiIntegrationTests()
    {
        _dbContainer = new MsSqlBuilder()
            .WithPassword("Password123!")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start real SQL Server container
        await _dbContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(
                        services.Single(
                            d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext>)
                        )
                    );

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(_dbContainer.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    // ✅ POSITIVE TEST: POST /storage
    [Fact]
    public async Task PostStorage_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/storage", new
        {
            key = "name",
            value = "Sushmitha"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // ✅ POSITIVE TEST: GET /storage/{key}
    [Fact]
    public async Task GetStorage_ReturnsStoredValue()
    {
        await _client.PostAsJsonAsync("/storage", new
        {
            key = "city",
            value = "Stockholm"
        });

        var response = await _client.GetAsync("/storage/city");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Stockholm");
    }

    // ❌ NEGATIVE TEST: GET unknown key
    [Fact]
    public async Task GetUnknownKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/storage/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ❌ NEGATIVE TEST: POST invalid body
    [Fact]
    public async Task PostInvalidBody_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/storage", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
