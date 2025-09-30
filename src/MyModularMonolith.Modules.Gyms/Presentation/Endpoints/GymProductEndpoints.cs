using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Presentation.Models;
using MyModularMonolith.Shared.Presentation;

namespace MyModularMonolith.Modules.Gyms.Presentation.Endpoints;

public static class GymProductEndpoints
{
    public static void MapGymProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gyms/{gymId:guid}/products")
            .WithTags("Gym Products");

        group.MapGet("/", GetGymProducts)
            .WithName("GetGymProducts")
            .WithSummary("Get products for a specific gym")
            .WithDescription("Retrieves all products available at a specific gym with pricing and configuration")
            .Produces<GymProductsListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", AddProductToGym)
            .WithName("AddProductToGym")
            .WithSummary("Add a product to a gym")
            .WithDescription("Assigns a product to a gym with specific pricing and configuration")
            .Produces<GymProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetGymProducts(
        Guid gymId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetGymProductsQuery(gymId, IncludeInactive: false);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(GymProductsListResponse.FromDtos(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> AddProductToGym(
        Guid gymId,
        [FromBody] AddProductToGymRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new AddProductToGymCommand(
            gymId,
            request.ProductId,
            request.Price,
            request.DiscountPercentage,
            request.Schedule,
            request.MinCapacity,
            request.MaxCapacity,
            request.InstructorName,
            request.InstructorEmail,
            request.InstructorPhone,
            request.Notes,
            request.Equipment);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Created($"/api/gyms/{gymId}/products/{success.Id}", GymProductResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }
}
