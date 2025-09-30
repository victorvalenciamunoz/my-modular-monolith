using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetAvailableTimeSlotsQuery(
    Guid GymProductId,
    DateTime Date) : IRequest<ErrorOr<AvailableTimeSlotsDto>>; 