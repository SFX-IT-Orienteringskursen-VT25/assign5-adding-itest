using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using AdditionApi.Models;
using AdditionApi.IntegrationTests.Helpers;

namespace AdditionApi.IntegrationTests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong_password_123!")
            .Build();

        public Task InitializeAsync()
        {
            return _dbContainer.StartAsync();
        }

        public new Task DisposeAsync()
        {
            return _dbContainer.StopAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveDbContext<AppDbContext>();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(_dbContainer.GetConnectionString());
                });
                services.EnsureDbCreated<AppDbContext>();
            });
        }
    }
}