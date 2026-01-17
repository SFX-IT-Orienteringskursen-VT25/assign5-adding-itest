using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SetupMssqlExample;

public class IntegrationTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _webApplicationFactory;
    public WebApplicationFactory<Program> WebApplicationFactory => _webApplicationFactory
                                                                    ?? throw new InvalidOperationException("WebApplicationFactory has not been initialized.");
    //temporary create database and then delete, aviod using scoped database
    public void ResetDatabase()
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Database>();
        db.DeleteAll();
    }
    public async Task InitializeAsync()
    {
        await DockerStarter.StartDockerContainerAsync();

        var webApplicationFactory = CreateWebApplicationFactory();
        _webApplicationFactory = webApplicationFactory;
        using (var scope = webApplicationFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Database>();
            db.Setup();
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private WebApplicationFactory<Program> CreateWebApplicationFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:DefaultConnection", 
                    "Server=localhost,1433;Database=master;User Id=sa;Password=MyStrongPassword123!;TrustServerCertificate=True;"
                    }
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