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
        _cache = _cacheProvider.GetCache(GymsCacheKeys.CacheName);
        _cacheConfig = cacheConfig.Value;
    }

    public async Task<ErrorOr<List<GymDto>>> Handle(GetActiveGymsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetActiveGymsQuery");

        var cacheKey = GymsCacheKeys.ActiveGymsList;
        var result = await _cache.GetOrSetAsync<List<GymDto>>(
            cacheKey,
            async _ =>
            {
                _logger.LogInformation("Fetching active gyms from database");
                var activeOrderedSpec = new GetActiveGymsSpec();
                var gymEntities = await _gymRepository.ListAsync(activeOrderedSpec, cancellationToken);
                return gymEntities.Select(gym => new GymDto(
                                        gym.Id,
                                        gym.Name,
                                        gym.IsActive,
                                        gym.CreatedAt,
                                        gym.UpdatedAt)).ToList();
            },
            TimeSpan.FromMinutes(_cacheConfig.Durations.GymList)
            ,
            cancellationToken);

        return result;
    }
}
