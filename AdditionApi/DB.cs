using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;

namespace AdditionApi;

public static class Database
{
    public static string? ConnectionString { get; set; }
    private const string TableName = "Storage";
    private const string DbName = "AdditionApiDB";

    public static async Task SetupAsync()
    {
        Console.WriteLine("Wait");
        var maxRetries = 5;
        var retryCount = 0;
        var masterConnectionString = ConnectionString?.Replace(DbName, "master") ?? $"Server=localhost,1433;Database=master;User Id=sa;Password={DbCredentials.Password};TrustServerCertificate=True;";

        while (retryCount < maxRetries)
        {
            try
            {
                using var conn = new SqlConnection(masterConnectionString);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                IF DB_ID('{DbName}') IS NULL CREATE DATABASE {DbName};
                GO
                USE {DbName};
                IF OBJECT_ID(N'{TableName}', N'U') IS NULL
                BEGIN
                    CREATE TABLE {TableName} (
                        [Key] VARCHAR(255) PRIMARY KEY,
                        [Value] VARCHAR(MAX) NOT NULL
                    );
                END";

                cmd.CommandText = $"IF DB_ID('{DbName}') IS NULL CREATE DATABASE {DbName};";
                await cmd.ExecuteNonQueryAsync();

                using var tableConn = CreateConnection();
                using var tableCmd = tableConn.CreateCommand();
                tableCmd.CommandText = $@"
                    IF OBJECT_ID(N'{TableName}', N'U') IS NULL
                    BEGIN
                        CREATE TABLE {TableName} (
                            [Key] VARCHAR(255) PRIMARY KEY,
                            [Value] VARCHAR(MAX) NOT NULL
                        );
                    END";
                await tableCmd.ExecuteNonQueryAsync();

                Console.WriteLine("DB and Table verified.");
                return;
            }
            catch (SqlException ex) when (retryCount < maxRetries -1)
            {
                retryCount++;
                Console.WriteLine($"DB is not ready. Attemps: {retryCount}. Error: {ex.Message}");
                await Task.Delay(2000);
            }
            
        }
        throw new Exception("SQL was not ready in time");
    }

    public static void Clear()
    {
        using var sqlConnection = CreateConnection();
        using var instruction = sqlConnection.CreateCommand();

        // Use TRUNCATE to quickly remove all rows from the table
        instruction.CommandText = $"TRUNCATE TABLE {TableName};";
        
        instruction.ExecuteNonQuery();
    }

    public static async Task SetValue(string key, string value)
    {
        using var sqlConnection = CreateConnection();
        using var instruction = sqlConnection.CreateCommand();

        instruction.CommandText = $@" 
        INSERT INTO {TableName} ([Key], [Value]) VALUES (@key, @value);";

        instruction.Parameters.AddWithValue("@key", key);
        instruction.Parameters.AddWithValue("@value", value);
        instruction.ExecuteNonQuery();
    }

    public static string? GetValue(string key)
    {
        using var sqlConnection = CreateConnection();
        using var instruction = sqlConnection.CreateCommand();

        instruction.CommandText = $@" 
        SELECT [Value] FROM {TableName} WHERE [Key] = @key;";

        instruction.Parameters.AddWithValue("@key", key);
        
        using var reader = instruction.ExecuteReader();
        if (reader.Read())
        {
            return reader["Value"].ToString();
        }
        
        return null;
    }

    public static bool KeyExists(string key)
    {
        using var sqlConnection = CreateConnection();
        using var instruction = sqlConnection.CreateCommand();
        instruction.CommandText = $@"
        SELECT COUNT(*) FROM {TableName} WHERE [Key] = @key;";

        instruction.Parameters.AddWithValue("@key", key);
        
        var count = (int)instruction.ExecuteScalar()!;
        return count > 0;
    }

    private static SqlConnection CreateConnection()
    {
        var connStr = ConnectionString ?? $"Server=localhost,1433;Database={DbName};User Id=sa;Password={DbCredentials.Password};TrustServerCertificate=True;";
        
        var sqlConnection = new SqlConnection(connStr);
        sqlConnection.Open();

        return sqlConnection;
    }
}