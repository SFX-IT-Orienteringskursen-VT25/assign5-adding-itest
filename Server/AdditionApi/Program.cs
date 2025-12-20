using AdditionApi.Data;
using AdditionApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register DbContext (ONLY ONCE)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        "Server=localhost;Database=StorageDb;User Id=sa;Password=Password123!;TrustServerCertificate=True"
    ));

var app = builder.Build();

// ✅ Ensure DB exists / migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}



// POST /storage
app.MapPost("/storage", async (AppDbContext db, StorageItem? item) =>
{
    if (item == null ||
        string.IsNullOrWhiteSpace(item.Key) ||
        string.IsNullOrWhiteSpace(item.Value))
    {
        return Results.BadRequest();
    }

    db.StorageItems.Add(item);
    await db.SaveChangesAsync();

    // ✅ NO BODY — FIXES PIPEWRITER BUG
    return Results.Created($"/storage/{item.Key}", null);
});

// GET /storage/{key}
app.MapGet("/storage/{key}", async (AppDbContext db, string key) =>
{
    var item = await db.StorageItems.FirstOrDefaultAsync(x => x.Key == key);

    if (item == null)
    {
        return Results.NotFound();
    }

    // ✅ RETURN PLAIN TEXT (NOT JSON)
    return Results.Text(item.Value);
});

/* ===================================================== */

app.Run();

public partial class Program { } // REQUIRED for integration tests
