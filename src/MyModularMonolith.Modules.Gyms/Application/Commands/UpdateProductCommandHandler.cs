using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ErrorOr<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productSpec = new GetProductByIdSpec(request.Id);
        var product = await _productRepository.FirstOrDefaultAsync(productSpec, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Attempt to update non-existing product with ID: {ProductId}", request.Id);
            return Error.NotFound("Product.NotFound", $"Product with ID {request.Id} was not found");
        }

        product.UpdateDetails(
            request.Name,
            request.Description,
            request.BasePrice,
            request.RequiresSchedule,
            request.RequiresInstructor,
            request.HasCapacityLimits,
            request.MinimumRequiredMembership,
            _dateTimeProvider.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product with ID: {ProductId} updated successfully", request.Id);

        return ProductMapper.ToDto(product);
    }
}
