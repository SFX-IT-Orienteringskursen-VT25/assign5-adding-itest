using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using AdditionApi;
using Microsoft.Extensions.Logging;

namespace AdditionApi.IntegrationTests;

public class AdditionApiFactory : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _webApplicationFactory;

    public WebApplicationFactory<Program> WebApplicationFactory =>
        _webApplicationFactory ?? throw new InvalidOperationException("WebApplicationFactory has not been initialized");

    public async Task InitializeAsync()
    {
        // Start Docker container and setup database
        await DockerStarter.StartDockerContainerAsync();
        Database.Setup();

        // Create the web application factory
        var webApplicationFactory = CreateWebApplicationFactory();
        _webApplicationFactory = webApplicationFactory;
    }

    public async Task DisposeAsync()
    {
        if (_webApplicationFactory != null)
        {
            await _webApplicationFactory.DisposeAsync();
        }

        // Stop and remove the Docker container
        await DockerStarter.StopAndRemoveContainerAsync();
    }

    private WebApplicationFactory<Program> CreateWebApplicationFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning);
            });

            builder.ConfigureServices(services =>
            {
                // Service configuration for tests can be done here
            });

            builder.ConfigureTestServices(services =>
            {
                // Test specific service configuration can be done here
            });

            builder.UseTestServer();
        });
    }
}
