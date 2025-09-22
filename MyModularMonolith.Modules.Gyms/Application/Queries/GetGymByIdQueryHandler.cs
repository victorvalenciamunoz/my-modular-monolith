using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetGymByIdQueryHandler : IRequestHandler<GetGymByIdQuery, ErrorOr<GymDto>>
{
    private readonly IGymRepository _gymRepository;

    public GetGymByIdQueryHandler(IGymRepository gymRepository)
    {
        _gymRepository = gymRepository;
    }

    public async Task<ErrorOr<GymDto>> Handle(GetGymByIdQuery request, CancellationToken cancellationToken)
    {        
        var gymSpec = new GetGymByIdSpec(request.GymId);
        var gym = await _gymRepository.FirstOrDefaultAsync(gymSpec, cancellationToken);
        
        if (gym == null)
        {
            return Error.NotFound("Gym.NotFound", "Gym not found.");
        }

        return new GymDto(
            gym.Id,
            gym.Name,
            gym.IsActive,
            gym.CreatedAt,
            gym.UpdatedAt);
    }
}
