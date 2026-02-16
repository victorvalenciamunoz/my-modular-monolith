using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyModularMonolith.Modules.Gyms.Infrastructure;
using MyModularMonolith.Modules.Users.Infrastructure;
using Serilog;

namespace MyModularMonolith.Api.Extensions;

public static class DevelopmentExtensions
{
    /// <summary>
    /// Agrega endpoints útiles para desarrollo (solo en modo Development).
    /// </summary>
    public static IEndpointRouteBuilder MapDevelopmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/migrate", async (IServiceProvider services) =>
        {
            using var scope = services.CreateScope();

            var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            Log.Information("Starting Users module database migrations");
            await usersContext.Database.MigrateAsync();
            Log.Information("Users module database migrations applied");

            var gymsContext = scope.ServiceProvider.GetRequiredService<GymsDbContext>();
            Log.Information("Starting Gyms module database migrations");
            await gymsContext.Database.MigrateAsync();
            Log.Information("Gyms module database migrations applied");

            Log.Information("Database migrations completed successfully");
            return Results.Ok("Migrations completed successfully");
        })
        .WithName("RunMigrations")
        .WithTags("Database")
        .WithSummary("Run database migrations for all modules (Development only)");

        return app;
    }
}
