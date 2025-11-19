using System;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Xunit;

namespace IntegrationTests
{
    public class SqlTestContainerFixture : IAsyncLifetime
    {
        public MsSqlContainer Container { get; private set; } = default!;

        public string ConnectionString => Container.GetConnectionString();

        public async Task InitializeAsync()
        {
            Container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("yourStrong(!)Password")
                .Build();

            await Container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            if (Container != null)
            {
                await Container.StopAsync();
                await Container.DisposeAsync();
            }
        }
    }
}
