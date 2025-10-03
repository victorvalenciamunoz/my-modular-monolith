using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyModularMonolith.Modules.Gyms.Application.Cache;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Configuration;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Shared.Application;
using System;
using System.Collections.Generic;
using System.Text;
using ZiggyCreatures.Caching.Fusion;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class AddGymCommandHandler : IRequestHandler<AddGymCommand, ErrorOr<GymDto>>
{
    private readonly IGymRepository _gymRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFusionCacheProvider _cacheProvider;
    private readonly GymsCacheConfiguration _cacheConfig;
    private readonly ILogger<AddGymCommandHandler> _logger;
    private readonly IFusionCache _cache;

    public AddGymCommandHandler(
        IGymRepository gymRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IFusionCacheProvider cacheProvider,
        IOptions<GymsCacheConfiguration> cacheConfig,
        ILogger<AddGymCommandHandler> logger)
    {
        _gymRepository = gymRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _cacheProvider = cacheProvider;
        _cacheConfig = cacheConfig.Value;
        _logger = logger;
        _cache = _cacheProvider.GetCache(GymsCacheKeys.CacheName);
    }
    public async Task<ErrorOr<GymDto>> Handle(AddGymCommand request, CancellationToken cancellationToken)
    {
        try
        {            
            var gymByNameSpec = new GetGymByNameSpec(request.Name);
            var existingGym = await _gymRepository.FirstOrDefaultAsync(gymByNameSpec, cancellationToken);
            if (existingGym is not null)
            {
                return Error.Conflict("Gym.NameAlreadyExists", $"A gym with name '{request.Name}' already exists");
            }

            var gym = new Gym(request.Name, _dateTimeProvider.UtcNow);

            await _gymRepository.AddAsync(gym, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var gymDto = GymMapper.ToDto(gym);

            var cacheKey = GymsCacheKeys.GetGymById(gym.Id);
            await _cache.SetAsync(
                cacheKey,
                gymDto,
                TimeSpan.FromMinutes(_cacheConfig.Durations.Gym),
                cancellationToken);
                        
            await _cache.ExpireAsync(GymsCacheKeys.ActiveGymsList, token: cancellationToken);

            _logger.LogInformation("Created new gym with ID {GymId} and name '{GymName}'", gym.Id, gym.Name);

            return gymDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating gym with name '{GymName}'", request.Name);
            return Error.Failure("Gym.CreationFailed", "Failed to create gym");
        }
    }
}
