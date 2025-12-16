using AdditionApi.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using AdditionApi.Services;

namespace AdditionApi.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = DatabaseHelper.GetConnectionString(configuration);
        }

        public IEnumerable<Calculation> GetCalculations()
        {
            var items = new List<Calculation>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string query = @"
                SELECT TOP 10 Id, Operand1, Operand2, Operation, Result
                FROM Calculations
                ORDER BY Id DESC;";

            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new Calculation
                {
                    Id = reader.GetInt32(0),
                    Operand1 = reader.GetDouble(1),
                    Operand2 = reader.GetDouble(2),
                    Operation = reader.GetString(3),
                    Result = reader.GetDouble(4)
                });
            }

            return items;
        }

        public void SaveCalculation(Calculation calculation)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string query = @"
                INSERT INTO Calculations (Operand1, Operand2, Operation, Result)
                VALUES (@Operand1, @Operand2, @Operation, @Result);";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Operand1", calculation.Operand1);
            cmd.Parameters.AddWithValue("@Operand2", calculation.Operand2);
            cmd.Parameters.AddWithValue("@Operation", calculation.Operation);
            cmd.Parameters.AddWithValue("@Result", calculation.Result);

            cmd.ExecuteNonQuery();
        }
    }
}
