using Microsoft.EntityFrameworkCore;
using AdditionApi.Models;

namespace AdditionApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<StorageItem> StorageItems { get; set; } = null!;
}