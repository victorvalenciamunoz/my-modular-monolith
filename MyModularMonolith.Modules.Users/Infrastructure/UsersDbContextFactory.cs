using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace MyModularMonolith.Modules.Users.Infrastructure;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MyModularMonolith.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("6822a934-9b13-4e2e-9d6c-cbcaa5cac22e") // ✅ Tu User Secrets ID
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in User Secrets or appsettings.json");
        }

        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "Users");
        });

        return new UsersDbContext(optionsBuilder.Options);
    }
}
