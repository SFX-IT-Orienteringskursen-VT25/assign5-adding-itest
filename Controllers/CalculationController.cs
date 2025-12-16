using Microsoft.AspNetCore.Mvc;
using AdditionApi.Services;

namespace AdditionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly IDatabaseService _db;

        public CalculationController(IDatabaseService db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var calculations = _db.GetCalculations();
                return Ok(calculations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }
    }
}
