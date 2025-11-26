using AdditionApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdditionApi.Controllers;

[ApiController]
[Route("storage")]
public class StorageController : ControllerBase
{
    private readonly AppDbContext _db;

    public StorageController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> SetItem(StorageItem item)
    {
        var existing = await _db.StorageItems
            .FirstOrDefaultAsync(x => x.Key == item.Key);

        if (existing == null)
        {
            _db.StorageItems.Add(item);
        }
        else
        {
            existing.Value = item.Value;
        }

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetItem(string key)
    {
        var item = await _db.StorageItems
            .FirstOrDefaultAsync(x => x.Key == key);

        if (item == null)
            return NotFound();

        return Ok(item);
    }
}
