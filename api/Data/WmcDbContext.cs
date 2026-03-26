using Microsoft.EntityFrameworkCore;

namespace WMCApi.Data;

public class WmcDbContext : DbContext
{
    public WmcDbContext(DbContextOptions<WmcDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LocationItem> LocationItems => Set<LocationItem>();
    public DbSet<Flavor> Flavors => Set<Flavor>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();

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
            entity.HasOne(e => e.Flavor)
                  .WithMany()
                  .HasForeignKey(e => e.FlavorId);
            entity.HasOne(e => e.ProductType)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<Flavor>(entity =>
        {
            entity.ToTable("flavors");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.ToTable("product_types");
            entity.HasKey(e => e.Id);
        });
    }
}
