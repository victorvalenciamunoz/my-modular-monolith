using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyModularMonolith.Modules.Gyms.Presentation.Models;
using MyModularMonolith.Shared.Presentation;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;

namespace MyModularMonolith.Modules.Gyms.Presentation.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithTags("Reservations");
                
        group.MapPost("/", CreateReservation)
            .WithName("CreateReservation")
            .WithSummary("Create a new reservation")
            .WithDescription("Creates a reservation for a user on a specific gym product and time slot")
            .Produces<ReservationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
                
        group.MapDelete("/{reservationId:guid}", CancelReservation)
            .WithName("CancelReservation")
            .WithSummary("Cancel a reservation")
            .WithDescription("Cancels an existing reservation")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/user/{userId}", GetUserReservations)
            .WithName("GetUserReservations")
            .WithSummary("Get user reservations")
            .WithDescription("Retrieves all reservations for a specific user")
            .Produces<UserReservationsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/available-slots/{gymProductId:guid}", GetAvailableTimeSlots)
            .WithName("GetAvailableTimeSlots")
            .WithSummary("Get available time slots")
            .WithDescription("Retrieves available time slots for a gym product on a specific date")
            .Produces<AvailableTimeSlotsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
                
        group.MapGet("/gym-product/{gymProductId:guid}/slot", GetReservationsForSlot)
            .WithName("GetReservationsForSlot")
            .WithSummary("Get reservations for a specific time slot")
            .WithDescription("Retrieves all reservations for a gym product at a specific date and time")
            .Produces<SlotReservationsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
                
        group.MapPost("/{reservationId:guid}/confirm", ConfirmReservation)
            .WithName("ConfirmReservation")
            .WithSummary("Confirm a pending reservation")
            .WithDescription("Confirms a pending reservation, changing its status from Pending to Confirmed")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> CreateReservation(
        [FromBody] CreateReservationRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateReservationCommand(
            request.UserId,
            request.GymProductId,
            request.ReservationDateTime,
            request.UserNotes);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Created($"/api/reservations/{success.Id}", ReservationResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> CancelReservation(
        Guid reservationId,
        [FromBody] CancelReservationRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelReservationCommand(reservationId, request.UserId, request.CancellationReason);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Ok(ReservationResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> GetUserReservations(
        string userId,        
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserReservationsQuery(userId, IncludeCompleted:true);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(UserReservationsResponse.FromDtos(success)),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> GetAvailableTimeSlots(
        Guid gymProductId,
        [FromQuery] DateTime date,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {        
        if (date.Date < DateTime.UtcNow.Date)
        {
            return Results.BadRequest("Date cannot be in the past");
        }

        var query = new GetAvailableTimeSlotsQuery(gymProductId, date);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(AvailableTimeSlotsResponse.FromDto(success)), 
            errors => result.ToProblemDetails()
        );
    }
        
    private static async Task<IResult> GetReservationsForSlot(
        Guid gymProductId,
        [FromQuery] DateTime dateTime,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (dateTime < DateTime.UtcNow.AddHours(-1))
        {
            return Results.BadRequest("DateTime cannot be too far in the past");
        }

        var query = new GetReservationsForSlotQuery(gymProductId, dateTime);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match(
            success => Results.Ok(SlotReservationsResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }
        
    private static async Task<IResult> ConfirmReservation(
        Guid reservationId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new ConfirmReservationCommand(reservationId);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Ok(ReservationResponse.FromDto(success)),
            errors => result.ToProblemDetails()
        );
    }
}