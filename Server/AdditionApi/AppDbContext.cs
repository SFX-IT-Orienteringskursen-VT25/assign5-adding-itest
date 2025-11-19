using AdditionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AdditionApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StorageItem> StorageItems { get; set; } = null!;
    }
}
