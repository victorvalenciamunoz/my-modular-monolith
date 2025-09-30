using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyModularMonolith.Modules.AI.Agents;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Presentation.Models;
using MyModularMonolith.Shared.Presentation;

namespace MyModularMonolith.Modules.AI.Presentation.Endpoints;

public static class AIAnalysisEndpoints
{
    public static void MapAIAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai/reservations")
            .WithTags("AI Analysis");
                
        group.MapPost("/analyze/{gymProductId:guid}/slot", AnalyzeReservationsWithAI)
            .WithName("AnalyzeReservationsWithAI")
            .WithSummary("Analyze reservations using AI")
            .WithDescription("Uses AI to analyze reservations for a specific slot and provide recommendations")
            .Produces<ReservationAnalysisResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
                
        group.MapPost("/recommendations/{gymProductId:guid}/slot", GetAIRecommendations)
            .WithName("GetAIRecommendations")
            .WithSummary("Get AI recommendations for reservations")
            .WithDescription("Get AI-powered recommendations for managing reservations")
            .Produces<ReservationRecommendationsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
                       
        group.MapPost("/summary/{gymProductId:guid}/slot", GetAISummary)
            .WithName("GetAISummary")
            .WithSummary("Get AI summary of slot status")
            .WithDescription("Get AI-generated summary and insights for slot management")
            .Produces<AISummaryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> AnalyzeReservationsWithAI(
        Guid gymProductId,
        [FromQuery] DateTime dateTime,
        [FromServices] IMediator mediator,
        [FromServices] IReservationAgent reservationAgent,
        CancellationToken cancellationToken = default)
    {        
        var query = new GetReservationsForSlotQuery(gymProductId, dateTime);
        var slotResult = await mediator.Send(query, cancellationToken);

        if (slotResult.IsError)
        {
            return slotResult.ToProblemDetails();
        }
             
        var analysisResult = await reservationAgent.AnalyzeSlotReservationsAsync(slotResult.Value, cancellationToken);

        return analysisResult.Match(
            success => Results.Ok(ReservationAnalysisResponse.FromModel(success)),
            errors => analysisResult.ToProblemDetails()
        );
    }

    private static async Task<IResult> GetAIRecommendations(
        Guid gymProductId,
        [FromQuery] DateTime dateTime,
        [FromServices] IMediator mediator,
        [FromServices] IReservationAgent reservationAgent,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationsForSlotQuery(gymProductId, dateTime);
        var slotResult = await mediator.Send(query, cancellationToken);

        if (slotResult.IsError)
        {
            return slotResult.ToProblemDetails();
        }

        var recommendationsResult = await reservationAgent.GetReservationRecommendationsAsync(slotResult.Value, cancellationToken);

        return recommendationsResult.Match(
            success => Results.Ok(ReservationRecommendationsResponse.FromModels(success)),
            errors => recommendationsResult.ToProblemDetails()
        );
    }

    private static async Task<IResult> GetAISummary(
        Guid gymProductId,
        [FromQuery] DateTime dateTime,
        [FromServices] IMediator mediator,
        [FromServices] IReservationAgent reservationAgent,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationsForSlotQuery(gymProductId, dateTime);
        var slotResult = await mediator.Send(query, cancellationToken);

        if (slotResult.IsError)
        {
            return slotResult.ToProblemDetails();
        }

        var summaryResult = await reservationAgent.GenerateSlotSummaryAsync(slotResult.Value, cancellationToken);

        return summaryResult.Match(
            success => Results.Ok(new AISummaryResponse(success, DateTime.UtcNow)),
            errors => summaryResult.ToProblemDetails()
        );
    }
}