using Microsoft.AspNetCore.Mvc;
using SetupMssqlExample; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<Database>(); 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin() 
                  .AllowAnyMethod()  
                  .AllowAnyHeader();
        });
});
builder.Services.AddScoped<Database>();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Database>();
    db.Setup();
}

app.UseCors("AllowAll");

// API 1: GET /number
app.MapGet("/number", ([FromServices] Database db) =>
{
    try 
    {
        var numbers = db.GetAllNumbers();
        return Results.Ok(numbers); 
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// API 2: POST /number
app.MapPost("/number", ([FromBody] NumberInput input, [FromServices] Database db) =>
{
    try
    {
        int newSum = input.Value;
        Console.WriteLine($"get input number: {newSum}");

        db.InsertValue(newSum);

        return Results.Ok("Saved successfully");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

internal record NumberInput(int Value);

namespace SetupMssqlExample
{
    public partial class Program { }
}