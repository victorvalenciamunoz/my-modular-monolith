using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetReservationsForSlotQuery(
    Guid GymProductId,
    DateTime SlotDateTime) : IRequest<ErrorOr<SlotReservationsDto>>;
