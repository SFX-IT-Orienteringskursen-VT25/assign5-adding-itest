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


//app.UseHttpsRedirection();

//var store = new ConcurrentDictionary<string, string?>();
//Create two endpoints that are suitable to replace localStorage in the previous assignment (persisted-addition) one for localStorage.setItem and another for localStorage.getItem

// app.MapGet("/localStorage/getItem/{key}", (string key) =>
// {
//     return store.TryGetValue(key, out var value)
//         ? Results.Ok(new { value })
//         : Results.Ok(new { value = (string?)null });
// });
// app.MapPut("/localStorage/setItem/{key}", async (string key ,[FromBody] SetItemRequest? body) =>
// {
//     //var body = await request.ReadFromJsonAsync<SetItemRequest>();
//     if (body is null) return Results.BadRequest("Body required");
//     store[key] = body.Value;
//     return Results.NoContent();
// });
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

