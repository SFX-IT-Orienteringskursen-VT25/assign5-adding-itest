using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using AdditionApi.Models;
using System.Collections.Generic;

namespace AdditionApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StorageController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("SaveCalculation")]
        public IActionResult SaveCalculation([FromBody] Calculation sqlCalculation)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseHelper.GetConnectionString(_configuration));
                conn.Open();

                const string query = @"
                    INSERT INTO Calculations (Operand1, Operand2, Operation, Result)
                    VALUES (@Operand1, @Operand2, @Operation, @Result);";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Operand1", sqlCalculation.Operand1);
                cmd.Parameters.AddWithValue("@Operand2", sqlCalculation.Operand2);
                cmd.Parameters.AddWithValue("@Operation", sqlCalculation.Operation);
                cmd.Parameters.AddWithValue("@Result", sqlCalculation.Result);

                cmd.ExecuteNonQuery();

                return Ok("Calculation saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving calculation: {ex.Message}");
            }
        }

        [HttpGet("GetCalculations")]
        public IActionResult GetCalculations()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseHelper.GetConnectionString(_configuration));
                conn.Open();

                const string query = @"
                    SELECT TOP 10 Id, Operand1, Operand2, Operation, Result
                    FROM Calculations
                    ORDER BY Id DESC;";

                using var cmd = new SqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                var items = new List<object>();
                while (reader.Read())
                {
                    items.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Operand1 = reader.GetDouble(1),
                        Operand2 = reader.GetDouble(2),
                        Operation = reader.GetString(3),
                        Result = reader.GetDouble(4)
                    });
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving calculations: {ex.Message}");
            }
        }
    }
}
