using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyModularMonolith.Modules.Users.Application.Services;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Modules.Users.Endpoints;
using MyModularMonolith.Modules.Users.Infrastructure;
using MyModularMonolith.Modules.Users.Infrastructure.Services;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Shared.Infrastructure;
using System.Text;

namespace MyModularMonolith.Modules.Users;

public static class UsersModuleExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, List<System.Reflection.Assembly> mediatRAssemblies)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<UsersDbContext>()        
        .AddDefaultTokenProviders();

        services.AddAuthorization();

        services.AddOptions<JwtOptions>()
           .BindConfiguration(JwtOptions.SectionName);

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            new UnitOfWork<UsersDbContext>(provider.GetRequiredService<UsersDbContext>()));

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IUserMetricsService, UserMetricsService>();


        mediatRAssemblies.Add(typeof(UsersModuleExtensions).Assembly);

        return services;
    }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        UsersEndpoints.MapUsersEndpoints(app);
        return app;
    }
}