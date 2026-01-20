using Microsoft.Data.SqlClient;
namespace AdditionApi.Repository;
using AdditionApi;

public class AdditionRepository : IAdditionRepository
{
    private const string TableName = "EnteredNumbers";
    private const string DbName = "AdditionDb";

    private static SqlConnection CreateConnection()
    {
        var sqlConnection = new SqlConnection($"Server=localhost,1433;Database=master;User Id=sa;Password={SqlCredentials.Password};TrustServerCertificate=True;");
        sqlConnection.Open();

        return sqlConnection;
    }
    public void InsertValue(string value)
    {
        using var sqlConnection = CreateConnection();
        using var insertCommand = sqlConnection.CreateCommand();
        insertCommand.CommandText = $@"
        USE {DbName};
        INSERT INTO {TableName} ([Value]) VALUES (@value);";
        insertCommand.Parameters.AddWithValue("@value", value);
        insertCommand.ExecuteNonQuery();
    }

    public List<string> Select()
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
            rowsInDb.Add(value);
        }

        Console.WriteLine("Rows in database: " + rowsInDb.Count);
        return rowsInDb;}

    public void DeleteAll()
    {
        using var sqlConnection = CreateConnection();
        using var insertCommand = sqlConnection.CreateCommand();
        insertCommand.CommandText = $@"
        USE {DbName};
        DELETE FROM {TableName};";
        insertCommand.ExecuteNonQuery();
    }

}

public interface IAdditionRepository
{
    
    void InsertValue(string value);
    List<string> Select();
    void DeleteAll();

}