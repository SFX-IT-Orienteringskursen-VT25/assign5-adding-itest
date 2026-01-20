using Microsoft.Data.SqlClient;

namespace AdditionApi;

public static class Database
{
    private const string TableName = "EnteredNumbers";
    private const string DbName = "AdditionDb";

    public static void Setup()
    {
        using var sqlConnection = CreateConnection();
        using var createDbCommand = sqlConnection.CreateCommand();
        createDbCommand.CommandText = $"IF DB_ID('{DbName}') IS NULL CREATE DATABASE {DbName};";
        createDbCommand.ExecuteNonQuery();

        using var createTableCommand = sqlConnection.CreateCommand();
        createTableCommand.CommandText = $@"
        USE {DbName};
        IF OBJECT_ID(N'{TableName}', N'U') IS NULL
        BEGIN
            CREATE TABLE {TableName} (
                [Id] INT PRIMARY KEY IDENTITY,
                [Value] VARCHAR(50) NOT NULL
            );
        END";
        createTableCommand.ExecuteNonQuery();
    }

    
    private static SqlConnection CreateConnection()
    {
        var sqlConnection = new SqlConnection($"Server=localhost,1433;Database=master;User Id=sa;Password={SqlCredentials.Password};TrustServerCertificate=True;");
        sqlConnection.Open();

        return sqlConnection;
    }
}