using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace sqlapi_integrationtest;

public class Database
{
    private const string TableName = "MyTable";
    private const string DbName = "MyDatabase";
    
    private readonly IConfiguration _configuration;

    public Database(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Setup()
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
                [Value] INT NOT NULL
            );
        END";
        createTableCommand.ExecuteNonQuery();
    }

    public void InsertValue(int value)
    {
        using var sqlConnection = CreateConnection();
        using var insertCommand = sqlConnection.CreateCommand();
        insertCommand.CommandText = $@"
        USE {DbName};
        INSERT INTO {TableName} ([Value]) VALUES (@value);";
        insertCommand.Parameters.AddWithValue("@value", value);
        insertCommand.ExecuteNonQuery();
    }

    // calculate sum of input
    public int GetSum()
    {
        using var sqlConnection = CreateConnection();
        using var cmd = sqlConnection.CreateCommand();
        
        // use SUM in SQL
        cmd.CommandText = $@"USE {DbName}; SELECT SUM(Value) FROM {TableName};";
        
        // ExecuteScalar write results
        var result = cmd.ExecuteScalar();

        // check if result is DBNull
        if (result == DBNull.Value || result == null)
        {
            return 0;
        }

        return Convert.ToInt32(result);
    }
    // for persisted number
    public List<int> GetAllNumbers()
    {
        using var sqlConnection = CreateConnection();
        using var cmd = sqlConnection.CreateCommand();

        cmd.CommandText = $@"USE {DbName}; SELECT Value FROM {TableName};";
        
        using var reader = cmd.ExecuteReader();
        var list = new List<int>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                list.Add(reader.GetInt32(0));
            }
        }

        return list;
    }
    public void Select()
    {
        using var sqlConnection = CreateConnection();
        using var insertCommand = sqlConnection.CreateCommand();
        insertCommand.CommandText = $@"
        USE {DbName};
        SELECT * FROM {TableName};";
        using var reader = insertCommand.ExecuteReader();

        var rowsInDb = new List<string>();
        while (reader.Read())
        {
            var id = reader["Id"].ToString();
            var value = reader["Value"].ToString();
            rowsInDb.Add($"{id}: {value}");
        }

        Console.WriteLine("Rows in database: " + rowsInDb.Count);
    }

    public void DeleteAll()
    {
        using var sqlConnection = CreateConnection();
        using var insertCommand = sqlConnection.CreateCommand();
        insertCommand.CommandText = $@"
        USE {DbName};
        DELETE FROM {TableName};";
        insertCommand.ExecuteNonQuery();
    }

    private SqlConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");
        }
        var sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();

        return sqlConnection;
    }
}