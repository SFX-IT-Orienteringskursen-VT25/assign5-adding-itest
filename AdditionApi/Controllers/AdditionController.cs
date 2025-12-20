using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdditionApi.Models;

namespace AdditionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdditionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdditionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var calculations = await _context.Calculations.ToListAsync();
            return Ok(calculations);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddRequest request)
        {
            int result = request.A + request.B;

            var calculation = new Calculation
            {
                Operand1 = request.A,
                Operand2 = request.B,
                Operation = "Addition",
                Result = result
            };

            _context.Calculations.Add(calculation);
            await _context.SaveChangesAsync();

            return Ok(calculation);
        }
    }
}