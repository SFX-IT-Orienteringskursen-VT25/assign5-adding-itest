using Microsoft.Data.SqlClient;

namespace AdditionApi;

public static class Database
{
    private const string TableName = "Storage";
    private const string DbName = "AdditionApiDatabase";
    
    // Allow overriding the connection string for testing
    public static string? TestConnectionString { get; set; }

    public static async Task SetupAsync()
    {
        // Wait for SQL Server to be ready with retry logic
        Console.WriteLine("Waiting for SQL Server to be ready...");
        var maxRetries = 30;
        var retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
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
                        [Key] VARCHAR(255) PRIMARY KEY,
                        [Value] VARCHAR(MAX) NOT NULL
                    );
                END";
                createTableCommand.ExecuteNonQuery();
                
                Console.WriteLine("Database setup completed successfully.");
                return;
            }
            catch (SqlException ex) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                Console.WriteLine($"SQL Server not ready yet (attempt {retryCount}/{maxRetries}). Waiting...");
                await Task.Delay(2000); // Wait 2 seconds before retry
            }
        }
        
        throw new Exception("SQL Server failed to start after maximum retries");
    }

    public static void SetValue(string key, string value)
    {
        using var sqlConnection = CreateConnection();
        using var command = sqlConnection.CreateCommand();
        command.CommandText = $@"
        USE {DbName};
        IF EXISTS (SELECT 1 FROM {TableName} WHERE [Key] = @key)
            UPDATE {TableName} SET [Value] = @value WHERE [Key] = @key
        ELSE
            INSERT INTO {TableName} ([Key], [Value]) VALUES (@key, @value);";
        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@value", value);
        command.ExecuteNonQuery();
    }

    public static string? GetValue(string key)
    {
        using var sqlConnection = CreateConnection();
        using var command = sqlConnection.CreateCommand();
        command.CommandText = $@"
        USE {DbName};
        SELECT [Value] FROM {TableName} WHERE [Key] = @key;";
        command.Parameters.AddWithValue("@key", key);
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader["Value"].ToString();
        }
        
        return null;
    }

    public static bool KeyExists(string key)
    {
        using var sqlConnection = CreateConnection();
        using var command = sqlConnection.CreateCommand();
        command.CommandText = $@"
        USE {DbName};
        SELECT COUNT(*) FROM {TableName} WHERE [Key] = @key;";
        command.Parameters.AddWithValue("@key", key);
        
        var count = (int)command.ExecuteScalar()!;
        return count > 0;
    }

    private static SqlConnection CreateConnection()
    {
        var connectionString = TestConnectionString ?? 
            $"Server=localhost,1433;Database=master;User Id=sa;Password={SqlCredentials.Password};TrustServerCertificate=True;";
        
        var sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();

        return sqlConnection;
    }
}
