using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
public class CustomWebApplicationFactory(MsSqlContainer container) : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer _container = container;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultMsSqlConnection"] = _container.GetConnectionString()
                }
            );
        });
    }
}
