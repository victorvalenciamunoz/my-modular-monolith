using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record ConfirmReservationCommand(
Guid ReservationId) : IRequest<ErrorOr<ReservationDto>>;
