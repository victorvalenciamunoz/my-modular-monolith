using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetGymProductsQueryHandler : IRequestHandler<GetGymProductsQuery, ErrorOr<List<GymProductDto>>>
{
    private readonly IGymProductRepository _gymProductRepository;

    public GetGymProductsQueryHandler(IGymProductRepository gymProductRepository)
    {
        _gymProductRepository = gymProductRepository;
    }

    public async Task<ErrorOr<List<GymProductDto>>> Handle(GetGymProductsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetGymProductsByGymIdSpec(request.GymId, request.IncludeInactive);
        var gymProducts = await _gymProductRepository.ListAsync(spec, cancellationToken);
                
        var gymProductDtos = gymProducts
            .Select(ProductMapper.ToDto) 
            .ToList();

        return gymProductDtos;
    }
}