using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AdditionApi.Models
{
    public static class Database
    {
        public static void CreateAndSeedDatabase(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    IF OBJECT_ID('Calculations', 'U') IS NULL
                    CREATE TABLE Calculations (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Expression NVARCHAR(100),
                        Result FLOAT
                    )";

                SqlCommand createTableCmd = new SqlCommand(createTableQuery, connection);
                createTableCmd.ExecuteNonQuery();

                string seedDataQuery = @"
                    INSERT INTO Calculations (Expression, Result)
                    SELECT '1 + 1', 2
                    WHERE NOT EXISTS (SELECT 1 FROM Calculations WHERE Expression = '1 + 1');

                    INSERT INTO Calculations (Expression, Result)
                    SELECT '2 * 3', 6
                    WHERE NOT EXISTS (SELECT 1 FROM Calculations WHERE Expression = '2 * 3');";

                SqlCommand seedDataCmd = new SqlCommand(seedDataQuery, connection);
                seedDataCmd.ExecuteNonQuery();
            }
        }

        public static List<string> GetAllCalculations(string connectionString)
        {
            List<string> results = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT Expression, Result FROM Calculations";
                SqlCommand selectCmd = new SqlCommand(selectQuery, connection);

                using (SqlDataReader reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string expression = reader["Expression"].ToString();
                        string result = reader["Result"].ToString();
                        results.Add($"{expression} = {result}");
                    }
                }
            }

            return results;
        }
        public static string GetConnectionString()
        {
            return "Server=localhost,1433;Database=CalculationDb;User Id=sa;Password=Password123!";
        }

    }
}
