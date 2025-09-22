using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Shared.Application;
using System.Reflection;

namespace MyModularMonolith.Modules.Users.Infrastructure;

public class UsersDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public UsersDbContext(DbContextOptions<UsersDbContext> options, IDateTimeProvider dateTimeProvider) : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Users");

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
            options.MigrationsHistoryTable("__EFMigrationsHistory", "Users"));
    }
}