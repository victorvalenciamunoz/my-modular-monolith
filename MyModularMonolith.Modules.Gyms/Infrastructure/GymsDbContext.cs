using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Application;
using System.Reflection;

namespace MyModularMonolith.Modules.Gyms.Infrastructure;

public class GymsDbContext : DbContext
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DbSet<Gym> Gyms { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<GymProduct> GymProducts { get; set; } = null!;

    public DbSet<Reservation> Reservations { get; set; } = null!;
    public GymsDbContext(DbContextOptions<GymsDbContext> options, IDateTimeProvider dateTimeProvider) : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Gyms");

        base.OnModelCreating(modelBuilder);
                
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {            
            base.OnConfiguring(optionsBuilder);
        }
                
        optionsBuilder.UseSqlServer(options =>
            options.MigrationsHistoryTable("__EFMigrationsHistory", "Gyms"));
    }

}
