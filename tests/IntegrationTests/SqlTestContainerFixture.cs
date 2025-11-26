using System.Threading.Tasks;
using Testcontainers.MsSql;
using Xunit;

namespace IntegrationTests
{
    public class SqlTestContainerFixture : IAsyncLifetime
{
    public MsSqlContainer Container { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        Container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/azure-sql-edge")
            .WithPassword("YourStrong!Passw0rd")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_PID", "Developer")
            .Build();

        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container != null)
            await Container.DisposeAsync();
    }
    }

 }   
