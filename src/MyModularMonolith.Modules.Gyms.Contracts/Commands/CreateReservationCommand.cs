using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record CreateReservationCommand(
    string UserId,
    Guid GymProductId,
    DateTime ReservationDateTime,
    string? UserNotes = null) : IRequest<ErrorOr<ReservationDto>>;