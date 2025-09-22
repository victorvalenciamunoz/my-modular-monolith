using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Gyms.Infrastructure;

public class GymsDbContextFactory : IDesignTimeDbContextFactory<GymsDbContext>
{
    public GymsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("6822a934-9b13-4e2e-9d6c-cbcaa5cac22e") 
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MyModularMonolith.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<GymsDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in User Secrets or appsettings.json");
        }

        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "Gyms");
        });

        return new GymsDbContext(optionsBuilder.Options, new DateTimeProvider());
    }
}
