using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, ErrorOr<ReservationDto>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CancelReservationCommandHandler(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<ReservationDto>> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {        
        var reservationSpec = new GetReservationByIdWithNavigationSpec(request.ReservationId);
        var reservation = await _reservationRepository.FirstOrDefaultAsync(reservationSpec, cancellationToken);
        if (reservation == null)
        {
            return Error.NotFound("Reservation.NotFound", $"Reservation with ID {request.ReservationId} was not found");
        }
                
        if (reservation.UserId != request.UserId)
        {
            return Error.Forbidden("Reservation.NotOwner", "You can only cancel your own reservations");
        }
                
        var currentTime = _dateTimeProvider.UtcNow;
        if (!reservation.CanBeCancelled(currentTime))
        {
            return Error.Validation("Reservation.TooLateToCancel",
                "Reservations can only be cancelled at least 2 hours before the scheduled time");
        }

        try
        {            
            reservation.Cancel(request.CancellationReason, currentTime);

            await _reservationRepository.UpdateAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ReservationMapper.ToDto(reservation, reservation.GymProduct);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("Reservation.CancellationError", ex.Message);
        }
    }
}