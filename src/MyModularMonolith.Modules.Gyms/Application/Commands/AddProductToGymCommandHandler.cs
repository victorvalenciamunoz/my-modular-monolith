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

internal class AddProductToGymCommandHandler : IRequestHandler<AddProductToGymCommand, ErrorOr<GymProductDto>>
{
    private readonly IGymRepository _gymRepository;
    private readonly IProductRepository _productRepository;
    private readonly IGymProductRepository _gymProductRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<AddProductToGymCommandHandler> _logger;

    public AddProductToGymCommandHandler(
        IGymRepository gymRepository,
        IProductRepository productRepository,
        IGymProductRepository gymProductRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<AddProductToGymCommandHandler> logger)
    {
        _gymRepository = gymRepository;
        _productRepository = productRepository;
        _gymProductRepository = gymProductRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<GymProductDto>> Handle(AddProductToGymCommand request, CancellationToken cancellationToken)
    {
        var gymSpec = new GetGymByIdSpec(request.GymId);
        var gym = await _gymRepository.FirstOrDefaultAsync(gymSpec, cancellationToken);
        if (gym == null)
        {
            _logger.LogWarning("Attempt to add product to non-existing gym with ID: {GymId}", request.GymId);
            return Error.NotFound("Gym.NotFound", $"Gym with ID {request.GymId} was not found");
        }

        var productSpec = new GetProductByIdSpec(request.ProductId);
        var product = await _productRepository.FirstOrDefaultAsync(productSpec, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Attempt to add non-existing product with ID: {ProductId} to gym ID: {GymId}", request.ProductId, request.GymId);
            return Error.NotFound("Product.NotFound", $"Product with ID {request.ProductId} was not found");
        }

        var existingSpec = new GetGymProductByGymAndProductIdSpec(request.GymId, request.ProductId);
        var existingGymProduct = await _gymProductRepository.FirstOrDefaultAsync(existingSpec, cancellationToken);
        if (existingGymProduct != null)
        {
            _logger.LogWarning("Attempt to add already assigned product with ID: {ProductId} to gym ID: {GymId}", request.ProductId, request.GymId);
            return Error.Conflict("GymProduct.AlreadyExists", "This product is already assigned to the gym");
        }

        try
        {
            var gymProduct = new GymProduct(
                request.GymId,
                request.ProductId,
                request.Price,
                request.DiscountPercentage,
                _dateTimeProvider.UtcNow);

            gymProduct.SetProduct(product);

            if (!string.IsNullOrEmpty(request.Schedule) || request.MinCapacity.HasValue || request.MaxCapacity.HasValue)
            {
                gymProduct.SetSchedule(request.Schedule, request.MinCapacity, request.MaxCapacity, _dateTimeProvider.UtcNow);
            }

            if (!string.IsNullOrEmpty(request.InstructorName))
            {
                gymProduct.SetInstructor(request.InstructorName, request.InstructorEmail, request.InstructorPhone, _dateTimeProvider.UtcNow);
            }

            if (!string.IsNullOrEmpty(request.Notes) || !string.IsNullOrEmpty(request.Equipment))
            {
                gymProduct.SetAdditionalInfo(request.Notes, request.Equipment, _dateTimeProvider.UtcNow);
            }

            await _gymProductRepository.AddAsync(gymProduct, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var addedProdct = ProductMapper.ToDto(gymProduct, gym, product);
            _logger.LogInformation("Product added {@productoDto}", addedProdct);
            
            return addedProdct;
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("Schedule.Invalid", ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation("Schedule.Invalid", ex.Message);
        }
    }
}
