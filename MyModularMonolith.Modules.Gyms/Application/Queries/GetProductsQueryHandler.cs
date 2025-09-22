using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ErrorOr<List<ProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ErrorOr<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetActiveProductsSpec();

        var products = await _productRepository.ListAsync(spec, cancellationToken);

        var productDtos = products
            .Select(ProductMapper.ToDto) 
            .ToList();

        return productDtos;
    }
}