using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetActiveGymsQueryHandler : IRequestHandler<GetActiveGymsQuery, ErrorOr<List<GymDto>>>
{
    private readonly IGymRepository _gymRepository;

    public GetActiveGymsQueryHandler(IGymRepository gymRepository)
    {
        _gymRepository = gymRepository;
    }

    public async Task<ErrorOr<List<GymDto>>> Handle(GetActiveGymsQuery request, CancellationToken cancellationToken)
    {
        var activeOrderedSpec = new GetActiveGymsSpec();

        var gyms = await _gymRepository.ListAsync(activeOrderedSpec, cancellationToken);

        var gymDtos = gyms.Select(gym => new GymDto(
            gym.Id,
            gym.Name,
            gym.IsActive,
            gym.CreatedAt,
            gym.UpdatedAt)).ToList();

        return gymDtos;
    }
}
