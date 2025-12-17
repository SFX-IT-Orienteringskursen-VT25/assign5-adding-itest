using Microsoft.AspNetCore.Mvc;
using AdditionApi.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost]
        public async Task<IActionResult> Add([FromQuery] int a, [FromQuery] int b)
        {
            var result = a + b;

            var calculation = new Calculation
            {
                Operand1 = a,
                Operand2 = b,
                Result = result,
                Operation = "add"
            };

            _context.Calculations.Add(calculation);
            await _context.SaveChangesAsync();

            return Ok(calculation);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Calculations.ToListAsync();
            return Ok(list);
        }
    }
}