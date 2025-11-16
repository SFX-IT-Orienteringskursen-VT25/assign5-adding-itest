using AdditionApi;
using AdditionApi.Models;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await DockerStarter.StartDockerContainerAsync();
Console.WriteLine("Waiting for SQL Server to start...");

string connectionString = Database.GetConnectionString();
bool connected = false;
int retryCount = 0;
int maxRetries = 10;

while (!connected && retryCount < maxRetries)
{
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        connected = true;
        Console.WriteLine("Database is ready.");
    }
    catch (SqlException)
    {
        retryCount++;
        Console.WriteLine($"Retry {retryCount}/{maxRetries}: Waiting 2 seconds...");
        await Task.Delay(2000);
    }
}

if (!connected)
{
    Console.WriteLine("Failed to connect to the database after multiple attempts.");
    return;
}

Database.CreateAndSeedDatabase(connectionString);
Console.WriteLine("Database seeded.");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers(); 

app.Run();
