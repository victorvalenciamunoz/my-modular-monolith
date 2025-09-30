using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ErrorOr<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ErrorOr<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var productSpec = new GetProductByIdSpec(request.ProductId);
        var product = await _productRepository.FirstOrDefaultAsync(productSpec, cancellationToken);
        if (product == null)
        {
            return Error.NotFound("Product.NotFound", "Product not found.");
        }

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.BasePrice,
            product.RequiresSchedule,
            product.RequiresInstructor,
            product.HasCapacityLimits,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);
    }
}
