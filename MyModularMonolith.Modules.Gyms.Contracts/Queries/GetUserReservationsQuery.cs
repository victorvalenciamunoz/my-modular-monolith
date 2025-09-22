using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetUserReservationsQuery(
    string UserId,
    bool IncludeCompleted = false) : IRequest<ErrorOr<List<ReservationDto>>>;