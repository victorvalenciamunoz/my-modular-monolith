using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record CreateProductCommand(
    string Name,
    string Description,    
    decimal BasePrice,
    bool RequiresSchedule,
    bool RequiresInstructor,
    bool HasCapacityLimits) : IRequest<ErrorOr<ProductDto>>;
