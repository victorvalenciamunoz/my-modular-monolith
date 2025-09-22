using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetProductByIdQuery(Guid ProductId) : IRequest<ErrorOr<ProductDto>>;
