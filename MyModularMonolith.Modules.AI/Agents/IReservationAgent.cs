using ErrorOr;
using MyModularMonolith.Modules.AI.Models;
using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.Modules.AI.Agents;

public interface IReservationAgent
{
    Task<ErrorOr<ReservationAnalysis>> AnalyzeSlotReservationsAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<List<ReservationRecommendation>>> GetReservationRecommendationsAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<SlotAnalysis>> AnalyzeSlotOptimizationAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<string>> GenerateSlotSummaryAsync(
        SlotReservationsDto slotData,
        CancellationToken cancellationToken = default);
}