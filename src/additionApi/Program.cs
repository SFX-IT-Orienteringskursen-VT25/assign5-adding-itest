using Microsoft.AspNetCore.Mvc;
using PersistentNumbers;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Database>();

// Add services to the container.
builder.Services.AddOpenApi();

//Add Cors Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
            .AllowAnyOrigin() 
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//Apply the Cors policy
app.UseCors("AllowLocalhost");


//Initialise DB
var db = app.Services.GetRequiredService<Database>();
db.Setup();

app.MapGet("/", () =>
{
    return "Hello World!";
});

app.MapGet("/numbers", (Database db) =>
{
    var numbers = db.SelectNumbers();
    return Results.Json(new { savedNumbers = numbers }, statusCode: 200);
});


app.MapPost("/numbers", (Database db, NumberInput req) =>
{
    db.InsertValue(req.number);
    var numbers = db.SelectNumbers();
    return Results.Json(new { savedNumbers = numbers }, statusCode: 200);
});


app.Run();

public record NumberInput(int number);
