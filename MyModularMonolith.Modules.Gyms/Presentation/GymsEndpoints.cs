using Microsoft.AspNetCore.Routing;
using MyModularMonolith.Modules.Gyms.Presentation.Endpoints;

namespace MyModularMonolith.Modules.Gyms.Presentation;

public static class GymsEndpoints
{
    public static void MapGymsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGymEndpoints();
        app.MapProductEndpoints();
        app.MapGymProductEndpoints();
        app.MapReservationEndpoints();
    }
}

