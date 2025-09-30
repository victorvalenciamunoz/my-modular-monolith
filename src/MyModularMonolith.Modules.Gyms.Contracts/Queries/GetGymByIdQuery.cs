using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetGymByIdQuery(Guid GymId) : IRequest<ErrorOr<GymDto>>;
