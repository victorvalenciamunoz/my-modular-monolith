namespace MyModularMonolith.Modules.AI.Models;

public record SlotAnalysis(
    Guid GymProductId,
    DateTime SlotDateTime,
    string GymName,
    string ProductName,
    int CurrentReservations,
    int? MaxCapacity,
    List<ReservationRecommendation> Recommendations,
    string OverallStrategy, 
    decimal OptimalCapacityUtilization,
    List<string> Insights);