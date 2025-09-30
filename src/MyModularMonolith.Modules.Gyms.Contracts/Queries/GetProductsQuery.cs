using ErrorOr;
using MediatR;

namespace MyModularMonolith.Modules.Gyms.Contracts.Queries;

public record GetProductsQuery() : IRequest<ErrorOr<List<ProductDto>>>;
