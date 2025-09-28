using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyModularMonolith.Modules.Gyms.Application.Cache;
using MyModularMonolith.Modules.Gyms.Configuration;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using ZiggyCreatures.Caching.Fusion;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetGymByIdQueryHandler : IRequestHandler<GetGymByIdQuery, ErrorOr<GymDto>>
{
    private readonly IGymRepository _gymRepository;
    private readonly IFusionCacheProvider _cacheProvider;
    private readonly ILogger<GetGymByIdQueryHandler> _logger;
    private readonly IFusionCache _cache;
    private readonly GymsCacheConfiguration _cacheConfig;

    public GetGymByIdQueryHandler(IGymRepository gymRepository,
                                IFusionCacheProvider cacheProvider,
                                IOptions<GymsCacheConfiguration> cacheConfig,
                                ILogger<GetGymByIdQueryHandler> logger)
    {
        _gymRepository = gymRepository;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _cache = _cacheProvider.GetCache(GymsCacheKeys.CacheName);
        _cacheConfig = cacheConfig.Value;
    }

    public async Task<ErrorOr<GymDto>> Handle(GetGymByIdQuery request, CancellationToken cancellationToken)
    {   
        var gymSpec = new GetGymByIdSpec(request.GymId);
        var cacheKey = $"gym:{request.GymId}";
        try
        {
            var gym = await _cache.GetOrSetAsync(
                cacheKey,
                async _ =>
                {
                    _logger.LogInformation("Cache miss for gym {GymId}, fetching from database", request.GymId);
                    var gymEntity = await _gymRepository.FirstOrDefaultAsync(gymSpec, cancellationToken);
                    return new GymDto(
                           gymEntity!.Id,
                           gymEntity!.Name,
                           gymEntity!.IsActive,
                           gymEntity!.CreatedAt,
                           gymEntity!.UpdatedAt);
                },
                TimeSpan.FromMinutes(_cacheConfig.Durations.Gym),
                cancellationToken);

            if (gym is null)
            {
                _logger.LogWarning("Gym with ID {GymId} not found", request.GymId);
                return Error.NotFound("Gym.NotFound", "Gym not found");
            }

            _logger.LogDebug("Gym {GymId} retrieved from cache", request.GymId);
            return gym;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gym {GymId}", request.GymId);
                        
            var gymEntity = await _gymRepository.GetByIdAsync(gymSpec, cancellationToken);
            return new GymDto(
                           gymEntity!.Id,
                           gymEntity!.Name,
                           gymEntity!.IsActive,
                           gymEntity!.CreatedAt,
                           gymEntity!.UpdatedAt);
        }        
    }
}
