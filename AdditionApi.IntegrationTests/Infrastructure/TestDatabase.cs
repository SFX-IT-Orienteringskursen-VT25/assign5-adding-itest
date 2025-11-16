using Testcontainers.MsSql;
using Xunit;

namespace AdditionApi.IntegrationTests.Infrastructure;

public class TestDatabase : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public TestDatabase()
    {
        _container = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
