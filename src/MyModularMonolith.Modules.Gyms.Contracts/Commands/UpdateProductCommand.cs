using ErrorOr;
using MediatR;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Contracts.Commands;

public record UpdateProductCommand(
 Guid Id,
 string Name,
 string Description,
 decimal BasePrice,
 bool RequiresSchedule,
 bool RequiresInstructor,
 bool HasCapacityLimits,
 MembershipLevel MinimumRequiredMembership = MembershipLevel.Standard)
 : IRequest<ErrorOr<ProductDto>>;
