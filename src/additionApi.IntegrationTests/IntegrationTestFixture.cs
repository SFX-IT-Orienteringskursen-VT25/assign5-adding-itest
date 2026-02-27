using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersistentNumbers;

namespace AdditionApi.IntegrationTests;


public class IntegrationTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _webApplicationFactory;
    public WebApplicationFactory<Program> WebApplicationFactory => _webApplicationFactory
                                                                    ?? throw new InvalidOperationException("WebApplicationFactory has not been initialized.");
    public Database GetDatabase() => WebApplicationFactory.Services.GetRequiredService<Database>();

    public async Task InitializeAsync()
    {
        await DockerStarter.StartDockerContainerAsync();

        var webApplicationFactory = CreateWebApplicationFactory();
        _webApplicationFactory = webApplicationFactory;
        GetDatabase().Setup();
    }

    public Task DisposeAsync()
    {
        GetDatabase().DeleteAll();

        return Task.CompletedTask;
    }

    private WebApplicationFactory<Program> CreateWebApplicationFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // if you want to override any configuration settings for tests, do it here
                })
                .AddEnvironmentVariables()
                .Build();

            builder.UseConfiguration(config);

            builder.ConfigureServices(services =>
            {
                // any additional service configuration for tests can be done here
            });

            builder.ConfigureTestServices(services =>
            {
                // any test-specific service configuration can be done here
            });

            builder.UseTestServer();
        });
    }
}