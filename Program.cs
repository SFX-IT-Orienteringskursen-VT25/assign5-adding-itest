using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return "Hello World!";
});

// GET endpoint - replaces localStorage.getItem('enteredNumbers')
app.MapGet("/storage/enteredNumbers", async (ApplicationDbContext context) =>
{
    var item = await context.StorageItems.FirstOrDefaultAsync(x => x.Key == "enteredNumbers");
    if (item == null)
    {
        return Results.Ok(new { numbers = new int[0] });
    }
    
    var numbers = JsonSerializer.Deserialize<int[]>(item.Value);
    return Results.Ok(new { numbers });
});

// PUT endpoint - replaces localStorage.setItem('enteredNumbers', JSON.stringify(numbers))
app.MapPut("/storage/enteredNumbers", async ([FromBody] NumbersRequest request, ApplicationDbContext context) =>
{
    var json = JsonSerializer.Serialize(request.Numbers);
    
    var existingItem = await context.StorageItems.FirstOrDefaultAsync(x => x.Key == "enteredNumbers");
    
    if (existingItem != null)
    {
        existingItem.Value = json;
        existingItem.UpdatedAt = DateTime.UtcNow;
    }
    else
    {
        context.StorageItems.Add(new StorageItem
        {
            Key = "enteredNumbers",
            Value = json,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
    
    await context.SaveChangesAsync();
    
    return Results.Ok(new { numbers = request.Numbers });
});

app.Run();

// Record for the numbers request
public record NumbersRequest(int[] Numbers);

public partial class Program { }