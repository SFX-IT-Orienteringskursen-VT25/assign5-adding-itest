using AdditionApi;
using AdditionApi.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AdditionApi.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestDatabase _testDatabase = new();

    public CustomWebApplicationFactory()
    {
        var depsFile = Path.Combine(AppContext.BaseDirectory, "testhost.deps.json");
        if (!File.Exists(depsFile))
        {
            File.WriteAllText(depsFile, "{}");
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var testConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _testDatabase.ConnectionString
                })
                .Build();

            configBuilder.AddConfiguration(testConfig);
        });
    }

    public async Task InitializeAsync()
    {
        await _testDatabase.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _testDatabase.DisposeAsync();
    }
}
