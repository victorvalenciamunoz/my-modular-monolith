using ErrorOr;
using MediatR;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record CreateProductCommand(
    string Name,
    string Description,    
    decimal BasePrice,
    bool RequiresSchedule,
    bool RequiresInstructor,
    bool HasCapacityLimits,
    MembershipLevel MembershipLevel) : IRequest<ErrorOr<ProductDto>>;
