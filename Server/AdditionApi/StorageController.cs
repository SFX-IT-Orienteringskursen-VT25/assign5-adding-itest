using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdditionApi.Models;

namespace AdditionApi
{
    [ApiController]
    [Route("storage")]
    public class StorageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StorageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SetItem([FromBody] StorageItem item)
        {
            var existing = await _context.StorageItems.FirstOrDefaultAsync(s => s.Key == item.Key);
            if (existing != null)
            {
                existing.Value = item.Value; // we are updating
            }
            else
            {
                _context.StorageItems.Add(item); // adding a new one
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Item saved successfully" });
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetItem(string key)
        {
            var item = await _context.StorageItems.FirstOrDefaultAsync(s => s.Key == key);
            if (item == null)
                return NotFound(new { message = "Key not found" });

            return Ok(item);
        }
    }
}
