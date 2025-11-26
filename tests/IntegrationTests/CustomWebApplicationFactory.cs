using AdditionApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using Xunit;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer _container;

    public CustomWebApplicationFactory(MsSqlContainer container)
    {
        _container = container;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString()
            });
        });
    }
}
