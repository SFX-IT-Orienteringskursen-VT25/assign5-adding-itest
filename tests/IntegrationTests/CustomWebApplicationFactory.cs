// tests/IntegrationTests/CustomWebApplicationFactory.cs
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AdditionApi; // пространство имён твоего API (проверь)
// using AdditionApi.Models; // при необходимости

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqlTestContainerFixture _sqlFixture;

    public CustomWebApplicationFactory(SqlTestContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // Заменяем connection string в конфигурации, чтобы приложение использовало контейнер
            var dict = new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = _sqlFixture.ConnectionString
            };
            configBuilder.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            // Удаляем регистрацию оригинального DbContext (если есть), чтобы перерегистрировать с нашим connection string
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType != null && d.ServiceType.Name.Contains("DbContextOptions"));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Регистрируем контекст с connection string контейнера
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_sqlFixture.ConnectionString);
            });

            // Если нужно — применяем миграции при старте тестового host'а
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
        });
    }
}
