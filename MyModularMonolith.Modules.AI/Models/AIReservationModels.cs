using MyModularMonolith.Modules.AI.Models;

namespace MyModularMonolith.Modules.Gyms.Presentation.Models;

public record ReservationAnalysisResponse(
    Guid GymProductId,
    DateTime SlotDateTime,
    int TotalReservations,
    int PendingReservations,
    int ConfirmedReservations,
    int? MaxCapacity,
    bool IsOverbooked,
    decimal OverbookingPercentage,
    List<UserReservationAnalysisResponse> UserAnalyses,
    List<string> Recommendations,
    decimal ConfidenceScore,
    DateTime ResponseTimestamp)
{
    public static ReservationAnalysisResponse FromModel(ReservationAnalysis model) =>
        new(model.GymProductId, model.SlotDateTime, model.TotalReservations,
            model.PendingReservations, model.ConfirmedReservations, model.MaxCapacity,
            model.IsOverbooked, model.OverbookingPercentage,
            model.UserAnalyses.Select(UserReservationAnalysisResponse.FromModel).ToList(),
            model.Recommendations, model.ConfidenceScore, DateTime.UtcNow);
}

public record UserReservationAnalysisResponse(
    string UserId,
    Guid ReservationId,
    string Status,
    DateTime CreatedAt,
    decimal AttendanceProbability,
    int PriorityScore,
    string RecommendedAction,
    List<string> Reasons)
{
    public static UserReservationAnalysisResponse FromModel(UserReservationAnalysis model) =>
        new(model.UserId, model.ReservationId, model.Status, model.CreatedAt,
            model.AttendanceProbability, model.PriorityScore, model.RecommendedAction, model.Reasons);
}

public record ReservationRecommendationResponse(
    Guid ReservationId,
    string UserId,
    string RecommendedAction,
    decimal Confidence,
    List<string> Reasons,
    Dictionary<string, object> Metadata)
{
    public static ReservationRecommendationResponse FromModel(ReservationRecommendation model) =>
        new(model.ReservationId, model.UserId, model.RecommendedAction,
            model.Confidence, model.Reasons, model.Metadata);
}

public record ReservationRecommendationsResponse(
    List<ReservationRecommendationResponse> Recommendations,
    int TotalRecommendations,
    DateTime ResponseTimestamp)
{
    public static ReservationRecommendationsResponse FromModels(List<ReservationRecommendation> models) =>
        new(models.Select(ReservationRecommendationResponse.FromModel).ToList(),
            models.Count, DateTime.UtcNow);
}

public record AISummaryResponse(
    string Summary,
    DateTime ResponseTimestamp);