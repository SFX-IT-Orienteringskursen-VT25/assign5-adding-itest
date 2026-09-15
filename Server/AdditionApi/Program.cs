
using AdditionApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/storage", async (StorageItem item, AppDbContext db) =>
{
    var existingItem = await db.StorageItems
        .FirstOrDefaultAsync(x => x.Key == item.Key);

    if (existingItem != null)
    {
        existingItem.Value = item.Value;
    }
    else
    {
        db.StorageItems.Add(item);
    }

    await db.SaveChangesAsync();

    return Results.Ok(item);
});

app.MapGet("/storage/{key}", async (string key, AppDbContext db) =>
{
    var item = await db.StorageItems
        .FirstOrDefaultAsync(x => x.Key == key);

    if (item == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(item);
});

app.Run();

