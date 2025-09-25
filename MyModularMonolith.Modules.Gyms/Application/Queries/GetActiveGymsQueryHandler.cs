using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetActiveGymsQueryHandler : IRequestHandler<GetActiveGymsQuery, ErrorOr<List<GymDto>>>
{
    private readonly IGymRepository _gymRepository;
    private readonly ILogger<GetActiveGymsQueryHandler> _logger;

    public GetActiveGymsQueryHandler(IGymRepository gymRepository, ILogger<GetActiveGymsQueryHandler> logger)
    {
        _gymRepository = gymRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<List<GymDto>>> Handle(GetActiveGymsQuery request, CancellationToken cancellationToken)
    {        
        _logger.LogInformation("Handling GetActiveGymsQuery");

        var activeOrderedSpec = new GetActiveGymsSpec();

        var gyms = await _gymRepository.ListAsync(activeOrderedSpec, cancellationToken);

        var gymDtos = gyms.Select(gym => new GymDto(
            gym.Id,
            gym.Name,
            gym.IsActive,
            gym.CreatedAt,
            gym.UpdatedAt)).ToList();

        _logger.LogInformation("Retrieved {Count} active gyms", gymDtos.Count);

        return gymDtos;
    }
}
