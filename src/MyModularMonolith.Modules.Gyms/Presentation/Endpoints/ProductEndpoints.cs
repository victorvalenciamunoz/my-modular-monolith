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

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get all products")
            .WithDescription("Retrieves a list of all active products")
            .Produces<ProductsListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .WithDescription("Retrieves detailed information about a specific product")
            .Produces<ProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product in the system")
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetProducts(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery();
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(ProductsListResponse.FromDtos(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(ProductResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.BasePrice,
            request.RequiresSchedule,
            request.RequiresInstructor,
            request.HasCapacityLimits);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Created($"/api/products/{success.Id}", ProductResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }
}
