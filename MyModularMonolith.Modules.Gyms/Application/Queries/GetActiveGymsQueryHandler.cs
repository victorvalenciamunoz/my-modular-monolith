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

internal class GetActiveGymsQueryHandler : IRequestHandler<GetActiveGymsQuery, ErrorOr<List<GymDto>>>
{
    private readonly IGymRepository _gymRepository;
    private readonly ILogger<GetActiveGymsQueryHandler> _logger;
    private readonly IFusionCache _cache;
    private readonly GymsCacheConfiguration _cacheConfig;
    private readonly IFusionCacheProvider _cacheProvider;
    public GetActiveGymsQueryHandler(IGymRepository gymRepository,
                            IFusionCacheProvider cacheProvider,
                            IOptions<GymsCacheConfiguration> cacheConfig,
                            ILogger<GetActiveGymsQueryHandler> logger)
    {
        _gymRepository = gymRepository;        
        _cacheProvider = cacheProvider;
        _logger = logger;
        _cache = _cacheProvider.GetCache(GymsModuleExtensions.CACHE_NAME);
        _cacheConfig = cacheConfig.Value;
    }

    public async Task<ErrorOr<List<GymDto>>> Handle(GetActiveGymsQuery request, CancellationToken cancellationToken)
    {        
        _logger.LogInformation("Handling GetActiveGymsQuery");        

        var activeOrderedSpec = new GetActiveGymsSpec();

        var gyms = await _cache.GetOrSetAsync(
            GymsCacheKeys.ActiveGymsList,
            async _ =>
            {
                _logger.LogInformation("Cache miss for {CacheKey} - fetching from database", GymsCacheKeys.ActiveGymsList);
                var gymEntities = await _gymRepository.ListAsync(activeOrderedSpec, cancellationToken);
                var result = gymEntities.Select(gym => new GymDto(
                                        gym.Id,
                                        gym.Name,
                                        gym.IsActive,
                                        gym.CreatedAt,
                                        gym.UpdatedAt)).ToList();
                _logger.LogInformation("Retrieved {Count} gyms from database", result.Count);
                return result;
            },
            TimeSpan.FromMinutes(_cacheConfig.Durations.GymList),
            cancellationToken);

        _logger.LogInformation("Cache hit for {CacheKey} - returning {Count} gyms from cache", GymsCacheKeys.ActiveGymsList, gyms.Count);
        return gyms;
    }
}
