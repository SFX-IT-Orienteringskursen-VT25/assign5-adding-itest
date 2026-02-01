using AdditionApi;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();
await Database.SetupAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return "Addition API";
});

app.MapGet("/api/addition/{key}",(string key) =>
{   
    var value = Database.GetValue(key);
    if (value != null)
    {
        return Results.Ok(new {key, value});
    }

    return Results.NotFound(new { message = $"Key {key} not found" });
});

app.MapPost("/api/addition", async ([FromBody] StorageData storageData) =>
{
    bool isExistedKey = Database.KeyExists(storageData.Key);
    
    if (isExistedKey)
    {
        return Results.BadRequest(StatusCodes.Status400BadRequest);
    }
    
    await Database.SetValue(storageData.Key,storageData.Value);

    return Results.Created($"/api/addition", new { key = storageData.Key, value =  storageData.Value});});


app.Run();

record StorageData(string Key, string Value);

public partial class Program { }