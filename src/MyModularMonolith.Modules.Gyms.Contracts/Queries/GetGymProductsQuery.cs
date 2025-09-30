using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetGymProductsQuery(
    Guid GymId,
    bool IncludeInactive = false) : IRequest<ErrorOr<List<GymProductDto>>>;
