using Microsoft.EntityFrameworkCore;

namespace AdditionApi.Data;

public class StorageContext : DbContext
{
    public StorageContext(DbContextOptions<StorageContext> options) : base(options)
    {
    }

    public DbSet<StorageEntry> StorageEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StorageEntry>()
            .HasKey(e => e.Key);
    }
}
