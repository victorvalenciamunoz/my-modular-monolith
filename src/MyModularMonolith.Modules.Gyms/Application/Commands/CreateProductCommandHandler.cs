using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ErrorOr<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(
            request.Name,
            request.Description,
            request.BasePrice,
            request.RequiresSchedule,
            request.RequiresInstructor,
            request.HasCapacityLimits,
            _dateTimeProvider.UtcNow,
            request.MembershipLevel);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToDto(product);
    }
}
