using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using AdditionApi;

// Загружаем .env
Env.Load();

// Читаем переменные окружения
var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "Your_password123!";
var dbName = Environment.GetEnvironmentVariable("MSSQL_DB") ?? "MyAppDb";
var host = Environment.GetEnvironmentVariable("MSSQL_HOST") ?? "127.0.0.1";
var port = Environment.GetEnvironmentVariable("MSSQL_PORT") ?? "1433";

// Формируем строку подключения
var connectionString =
    $"Server={host},{port};Database={dbName};User Id=sa;Password={saPassword};TrustServerCertificate=True;";

// Создаём builder
var builder = WebApplication.CreateBuilder(args);
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Применяем миграции (если нужно)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Простейшие эндпоинты
app.MapGet("/", () => "Hello from API!");
app.MapGet("/add", (int a, int b) => a + b);

app.Run();

// 👇 Это нужно для тестов
namespace AdditionApi
{
    public partial class Program { }
}
