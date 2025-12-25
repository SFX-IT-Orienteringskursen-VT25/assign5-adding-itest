using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using Xunit;

namespace AdditionApi.IntegrationTests;

public class AdditionApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    public string ConnectionString => _dbContainer.GetConnectionString();

    public AdditionApiFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithPassword("YourStrong!Passw0rd")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Any additional test service configuration can go here
        });

        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Override the DockerStarter and Database initialization
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(sp => new TestDatabaseConfiguration(ConnectionString));
        });

        return base.CreateHost(builder);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Set the test connection string for the Database class
        Database.TestConnectionString = ConnectionString;
        
        await SetupDatabaseAsync();
    }

    private async Task SetupDatabaseAsync()
    {
        const string dbName = "AdditionApiDatabase";
        const string tableName = "Storage";

        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Create database
        using var createDbCommand = connection.CreateCommand();
        createDbCommand.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE {dbName};";
        await createDbCommand.ExecuteNonQueryAsync();

        // Create table
        using var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = $@"
            USE {dbName};
            IF OBJECT_ID(N'{tableName}', N'U') IS NULL
            BEGIN
                CREATE TABLE {tableName} (
                    [Key] VARCHAR(255) PRIMARY KEY,
                    [Value] VARCHAR(MAX) NOT NULL
                );
            END";
        await createTableCommand.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

public record TestDatabaseConfiguration(string ConnectionString);
