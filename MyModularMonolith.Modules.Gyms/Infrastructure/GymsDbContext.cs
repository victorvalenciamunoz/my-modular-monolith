using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.Gyms.Domain;
using System.Reflection;

namespace MyModularMonolith.Modules.Gyms.Infrastructure;

public class GymsDbContext : DbContext
{
    public DbSet<Gym> Gyms { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<GymProduct> GymProducts { get; set; } = null!;

    public DbSet<Reservation> Reservations { get; set; } = null!;
    public GymsDbContext(DbContextOptions<GymsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Gyms");

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
