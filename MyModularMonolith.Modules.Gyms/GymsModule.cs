using MediatR;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Infrastructure;
using MyModularMonolith.Modules.Gyms.Presentation;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Gyms;

public static class GymsModuleExtensions
{
    public static IServiceCollection AddGymsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(GymsModuleExtensions).Assembly);
        services.AddMediatR(typeof(GetGymByIdQuery).Assembly);

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
                
        services.AddDbContext<GymsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IGymRepository, GymRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IGymProductRepository, GymProductRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.AddScoped<IUnitOfWork>(provider =>
            new UnitOfWork<GymsDbContext>(provider.GetRequiredService<GymsDbContext>()));

        return services;
    }

    public static IEndpointRouteBuilder MapGymsEndpoints(this IEndpointRouteBuilder app)
    {
        GymsEndpoints.MapGymsEndpoints(app);
        return app;
    }
}
