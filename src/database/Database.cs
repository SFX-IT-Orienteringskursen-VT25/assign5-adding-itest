using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace PersistentNumbers;

public class Database
{
    private const string TableName = "Numbers";
    private const string DbName = "PersistentNumbers";

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
        using var cmd = sqlConnection.CreateCommand();
        cmd.CommandText = $@"
        USE {DbName};
        INSERT INTO {TableName} ([Value]) VALUES (@value);";
        cmd.Parameters.AddWithValue("@value", value);
        cmd.ExecuteNonQuery();
    }

   public List<int> SelectNumbers()
{
    var numbers = new List<int>();

    using var sqlConnection = CreateConnection();
    using var cmd = sqlConnection.CreateCommand();
    cmd.CommandText = $@"
        USE {DbName};
        SELECT [Value] FROM {TableName};";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        int num = reader.GetInt32(0);
            numbers.Add(num);
        
    }

    return numbers;
}

    public void DeleteAll()
    {
        using var sqlConnection = CreateConnection();
        using var cmd = sqlConnection.CreateCommand();
        cmd.CommandText = $@"
        USE {DbName};
        DELETE FROM {TableName};";
        cmd.ExecuteNonQuery();
    }

    private SqlConnection CreateConnection()
    {
        var sqlConnection = new SqlConnection(
            $"Server=localhost,1433;Database=master;User Id=sa;Password={SqlCredentials.Password};TrustServerCertificate=True;");
        sqlConnection.Open();
        return sqlConnection;
    }
}
