//using AdditionApi;
using SetupMssqlExample;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
await DockerStarter.StartDockerContainerAsync();
builder.Services.AddOpenApi();

var app = builder.Build();


Database.Setup();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapGet("/localStorage/getItem/{key}", (string key) =>
{
    var value = Database.GetValue(key); 
    return Results.Ok(new { value });
});

app.MapPut("/localStorage/setItem/{key}", (string key, [FromBody] SetItemRequest body) =>
{
    if (body is null) return Results.BadRequest("Body required");
    Database.UpsertValue(key, body.Value); 
    return Results.NoContent();
});
app.MapGet("/", () => "Hello World!");
app.Run();
public record SetItemRequest(string? Value);

public partial class Program { }