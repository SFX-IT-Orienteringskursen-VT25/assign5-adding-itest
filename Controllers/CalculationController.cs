using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using AdditionApi.Models;

namespace AdditionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => /api/calculation
    public class CalculationController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                string connectionString = Database.GetConnectionString();
                var calculations = Database.GetAllCalculations(connectionString); 
                return Ok(calculations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }
    }
}
