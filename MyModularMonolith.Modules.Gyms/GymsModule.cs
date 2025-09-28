using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyModularMonolith.Modules.Gyms.Application.Cache;
using MyModularMonolith.Modules.Gyms.Configuration;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Infrastructure;
using MyModularMonolith.Modules.Gyms.Presentation;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Shared.Infrastructure;
using ZiggyCreatures.Caching.Fusion;

namespace MyModularMonolith.Modules.Gyms;

public static class GymsModuleExtensions
{
    public static IServiceCollection AddGymsModule(this IServiceCollection services, IConfiguration configuration, List<System.Reflection.Assembly> mediatRAssemblies)
    {
        services.AddMemoryCache();
        services.AddFusionCache(GymsCacheKeys.CacheName)
            .WithDefaultEntryOptions(options =>
            {
                var cacheSection = configuration.GetSection("Modules:Gyms:Cache");
                options.Duration = TimeSpan.FromMinutes(cacheSection.GetValue("DefaultDurationMinutes", 10));
                options.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal;
                options.Size = cacheSection.GetValue("DefaultSize", 1);
                options.JitterMaxDuration = TimeSpan.FromSeconds(cacheSection.GetValue("JitterMaxSeconds", 5));
            })
            .WithSystemTextJsonSerializer();

        services.Configure<GymsCacheConfiguration>(
            configuration.GetSection("Modules:Gyms:Cache"));

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IGymRepository, GymRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IGymProductRepository, GymProductRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.AddScoped<IUnitOfWork>(provider =>
            new UnitOfWork<GymsDbContext>(provider.GetRequiredService<GymsDbContext>()));

        mediatRAssemblies.Add(typeof(GymsModuleExtensions).Assembly);

        return services;
    }

    public static IEndpointRouteBuilder MapGymsEndpoints(this IEndpointRouteBuilder app)
    {
        GymsEndpoints.MapGymsEndpoints(app);
        return app;
    }
}
