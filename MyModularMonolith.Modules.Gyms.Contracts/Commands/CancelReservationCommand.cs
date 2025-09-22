using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record CancelReservationCommand(
    Guid ReservationId,
    string UserId,
    string CancellationReason) : IRequest<ErrorOr<ReservationDto>>;