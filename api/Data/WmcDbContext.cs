using Microsoft.EntityFrameworkCore;

namespace WMCApi.Data;

public class WmcDbContext : DbContext
{
    public WmcDbContext(DbContextOptions<WmcDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LocationItem> LocationItems => Set<LocationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("locations");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Items)
                  .WithOne(e => e.Location)
                  .HasForeignKey(e => e.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LocationItem>(entity =>
        {
            entity.ToTable("location_items");
            entity.HasKey(e => e.Id);
        });
    }
}
