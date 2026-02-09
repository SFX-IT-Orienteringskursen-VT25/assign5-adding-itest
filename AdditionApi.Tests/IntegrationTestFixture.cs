using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AdditionApi.Tests;

namespace AdditionApi.Tests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;

    public WebApplicationFactory<Program> Factory => _factory 
        ?? throw new InvalidOperationException("Fixture has not been initialized.");

    public HttpClient CreateClient() => Factory.CreateClient();

    public async Task InitializeAsync()
    {
        await DockerConfigSetup.StartDockerContainerAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Server=127.0.0.1,1433;User Id=sa;Password=password$123;TrustServerCertificate=True;"
                });
            });

            builder.UseTestServer();
        });

        await Database.SetupAsync(); 
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
        {
            Database.Clear();
            await _factory.DisposeAsync();
        }
    }
}