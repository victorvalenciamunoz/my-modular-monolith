using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Presentation.Models;
using MyModularMonolith.Shared.Presentation;

namespace MyModularMonolith.Modules.Gyms.Presentation.Endpoints;

public static class GymEndpoints
{
    public static void MapGymEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gyms")
            .WithTags("Gyms");

        group.MapGet("/", GetActiveGyms)
            .WithName("GetActiveGyms")
            .WithSummary("Get all active gyms")
            .WithDescription("Retrieves a list of all active gyms in the system")
            .Produces<GymsListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetGymById)
            .WithName("GetGymById")
            .WithSummary("Get gym by ID")
            .WithDescription("Retrieves detailed information about a specific gym")
            .Produces<GymResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetActiveGyms(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveGymsQuery();
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(GymsListResponse.FromDtos(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> GetGymById(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetGymByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(GymResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }
}
