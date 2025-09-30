namespace MyModularMonolith.Modules.AI.Models;

public record ReservationAnalysis(
    Guid GymProductId,
    DateTime SlotDateTime,
    int TotalReservations,
    int PendingReservations,
    int ConfirmedReservations,
    int? MaxCapacity,
    bool IsOverbooked,
    decimal OverbookingPercentage,
    List<UserReservationAnalysis> UserAnalyses,
    List<string> Recommendations,
    decimal ConfidenceScore);

public record UserReservationAnalysis(
    string UserId,
    Guid ReservationId,
    string Status,
    DateTime CreatedAt,
    decimal AttendanceProbability,
    int PriorityScore,
    string RecommendedAction, 
    List<string> Reasons);

public record ReservationRecommendation(
    Guid ReservationId,
    string UserId,
    string RecommendedAction,
    decimal Confidence,
    List<string> Reasons,
    Dictionary<string, object> Metadata);