using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Get / - Default route
app.MapGet("/", () =>
{
    return "assign3-addition-api";
});

// POST /api/storage - equivalent to localStorage.setItem
app.MapPost("/api/storage", async (AppDbContext appDbContext, Record record) =>
{
    if (string.IsNullOrWhiteSpace(record.Key))
        return Results.BadRequest("Key is required.");

    appDbContext.Records.Add(record);
    await appDbContext.SaveChangesAsync();
    return Results.Created($"/api/storage/{record.Key}", record);
});

// GET /api/storage/{key} - equivalent to localStorage.getItem
app.MapGet("/api/storage/{key}", async (AppDbContext appDbContext, string key) =>
{
    var result = await appDbContext.Records.FindAsync(key);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

// run the app
app.Run();