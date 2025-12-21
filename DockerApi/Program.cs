
using DockerApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MyDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Optional: start Docker container for DB (make sure DockerStarter is async-safe)
await DockerStarter.StartDockerContainerAsync();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapPost("/items", async (Item item, MyDbContext db) =>
{
    try
    {
        if (item.Value < 0)
            return Results.BadRequest("Value cannot be negative.");

        await db.Items.AddAsync(item);
        await db.SaveChangesAsync();

        return Results.Created("/items", item);
    }
    catch (Exception ex)
    {
        Console.WriteLine("🔥 ERROR IN POST /items:");
        Console.WriteLine(ex.ToString());
        throw;
    }
});


app.MapGet("/items", async (MyDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});

app.MapGet("/items/{id:int}", async (int id, MyDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

// Run the app
await app.RunAsync();
public partial class Program { }