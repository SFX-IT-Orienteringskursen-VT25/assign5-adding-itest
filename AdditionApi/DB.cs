using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;

namespace AdditionApi;
public static class Database
{
    private const string TableName = "Storage";
    private const string DbName = "AdditionApiDB";

    public static async Task SetupAsync()
    {
        Console.WriteLine("Wait");
        var maxRetries = 5;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                using var sqlConnection = CreateConnection();
                using var createDbInstruction = sqlConnection.CreateCommand();

                createDbInstruction.CommandText = $"IF DB_ID('{DbName}') IS NULL CREATE DATABASE {DbName};";
                createDbInstruction.ExecuteNonQuery();

                using var createDataTable = sqlConnection.CreateCommand();
                createDataTable.CommandText = $@"
                USE {DbName};
                IF OBJECT_ID(N'{TableName}', N'U') IS NULL
                BEGIN
                    CREATE TABLE {TableName} (
                        [Key] VARCHAR(255) PRIMARY KEY,
                        [Value] VARCHAR(MAX) NOT NULL
                    );
                END";
                createDataTable.ExecuteNonQuery();

                Console.WriteLine("DB created");
                return;
            }
            catch (SqlException) when (retryCount < maxRetries -1)
            {
                retryCount++;
                Console.WriteLine($"DB is not ready. Attemps: {retryCount}/{maxRetries}. Please, wait");
                await Task.Delay(2000);
            }
            
        }
        throw new Exception("DB failed to start");
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
        var sqlConnection = new SqlConnection($"Server=localhost,1433;Database={DbName};User Id=sa;Password={DbCredentials.Password};TrustServerCertificate=True;");
        sqlConnection.Open();

        return sqlConnection;
    }
}