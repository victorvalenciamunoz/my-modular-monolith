using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetUserReservationsQueryHandler : IRequestHandler<GetUserReservationsQuery, ErrorOr<List<ReservationDto>>>
{
    private readonly IReservationRepository _reservationRepository;

    public GetUserReservationsQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<ErrorOr<List<ReservationDto>>> Handle(GetUserReservationsQuery request, CancellationToken cancellationToken)
    {        
        var spec = new UserReservationsSpec(request.UserId, request.IncludeCompleted);
        var reservations = await _reservationRepository.ListAsync(spec, cancellationToken);

        var reservationDtos = reservations
            .Select(r => ReservationMapper.ToDto(r, r.GymProduct))
            .ToList();

        return reservationDtos;
    }
}