using Testcontainers.MsSql;

public class SqlContainerFactory : IAsyncLifetime
{
    public MsSqlContainer SqlContainer { get; } =
        new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithName("sqlserver-test-container")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithPassword("Your_password123")
            .WithEnvironment("MSSQL_PID", "Developer")
            .WithPortBinding(11433, 1433)   // hostPort, containerPort
            .WithCleanUp(true)
            .Build();

    public async Task InitializeAsync()
    {
        await SqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await SqlContainer.DisposeAsync();
    }
}
