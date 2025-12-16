using Microsoft.AspNetCore.Mvc;
using AdditionApi.Models;
using AdditionApi.Services;

namespace AdditionApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IDatabaseService _db;

        public StorageController(IDatabaseService db)
        {
            _db = db;
        }

        [HttpPost("SaveCalculation")]
        public IActionResult SaveCalculation([FromBody] Calculation calculation)
        {
            if (calculation == null)
            {
                return BadRequest();
            }

            try
            {
                _db.SaveCalculation(calculation);
            }
            catch
            {

            }

            return Ok();
        }

        [HttpGet("GetCalculations")]
        public IActionResult GetCalculations()
        {
            try
            {
                var data = _db.GetCalculations();
                return Ok(data);
            }
            catch
            {
                return Ok(new List<Calculation>());
            }
        }
    }
}
