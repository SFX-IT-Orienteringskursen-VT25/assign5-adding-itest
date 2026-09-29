using AdditionApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StorageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StorageContext>();
    db.Database.EnsureCreated();
}

app.MapPut("/storage/{key}", async (string key, [FromBody] StorageValue body, StorageContext db) =>
{
    if (key.Length > MaxKeyLength)
    {
        return Results.BadRequest($"Key must be at most {MaxKeyLength} characters.");
    }

    if (body.Value is null)
    {
        return Results.BadRequest("Value is required.");
    }

    var entry = await db.StorageEntries.FirstOrDefaultAsync(e => e.Key == key);

    if (entry == null)
    {
        db.StorageEntries.Add(new StorageEntry { Key = key, Value = body.Value });
    }
    else
    {
        entry.Value = body.Value;
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/storage/{key}", async (string key, StorageContext db) =>
{
    var entry = await db.StorageEntries.FirstOrDefaultAsync(e => e.Key == key);
    if (entry is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new StorageValue(entry.Value));
});

app.Run();

record StorageValue(string Value);

// Exposes the entry point to WebApplicationFactory in the integration tests.
public partial class Program
{
    // SQL Server limits nvarchar primary keys to 450 characters.
    private const int MaxKeyLength = 450;
}

