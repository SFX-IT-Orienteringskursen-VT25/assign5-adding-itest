using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using DockerApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace ApiUnitTests
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private WebApplicationFactory<Program>? _webApplicationFactory;

        public WebApplicationFactory<Program> WebApplicationFactory =>
            _webApplicationFactory ?? throw new InvalidOperationException("WebApplicationFactory has not been initialized");

        public async Task InitializeAsync()
        {

            await DockerStarter.StartDockerContainerAsync();
            //Database.Setup();
            var webApplicationFactory = CreateWebApplicationFactory();
            _webApplicationFactory = webApplicationFactory;
            using var scope = _webApplicationFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();


        }

        public async Task DisposeAsync()
        {
            // Database.DeleteAll(); // Not needed with in-memory DB

            if (_webApplicationFactory != null)
            {
                await _webApplicationFactory.DisposeAsync();
            }
        }

        private WebApplicationFactory<Program> CreateWebApplicationFactory()
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Server=localhost,1433;Database=ItemsDB;User Id=sa;Password=Password123!;TrustServerCertificate=true;"
                    })

                    .AddEnvironmentVariables()
                    .Build();

                builder.UseConfiguration(config);
                builder.ConfigureLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Critical);
});

                builder.ConfigureServices(services =>
                {
                    // service configuration for test can be done here

                });

                builder.ConfigureTestServices(services =>
                {
                    // test specific service configuration  can be done here


                });

                builder.UseTestServer();
            });
        }


    }
}