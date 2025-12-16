using AdditionApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace AdditionApi.IntegrationTests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<AdditionApi.Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        return base.CreateHost(builder);
    }
}
