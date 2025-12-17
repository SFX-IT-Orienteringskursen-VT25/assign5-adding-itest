using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdditionApi.IntegrationTests.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static void RemoveDbContext<T>(this IServiceCollection services) where T : DbContext
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<T>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        }

        public static void EnsureDbCreated<T>(this IServiceCollection services) where T : DbContext
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var context = serviceProvider.GetRequiredService<T>();
            context.Database.EnsureCreated();
        }
    }
}