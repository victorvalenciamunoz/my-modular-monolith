using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Shared.Application;

namespace MyModularMonolith.Modules.Gyms.Application.Commands
{
    internal class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, ErrorOr<ReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ConfirmReservationCommandHandler(
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ErrorOr<ReservationDto>> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
        {            
            var reservationSpec = new GetReservationByIdWithNavigationSpec(request.ReservationId);
            var reservation = await _reservationRepository.FirstOrDefaultAsync(reservationSpec, cancellationToken);

            if (reservation == null)
            {
                return Error.NotFound("Reservation.NotFound",
                    $"Reservation with ID {request.ReservationId} was not found");
            }
         
            var currentTime = _dateTimeProvider.UtcNow;
            if (!reservation.CanBeConfirmed(currentTime))
            {
                return Error.Validation("Reservation.CannotConfirm",
                    "This reservation cannot be confirmed. It may already be confirmed, cancelled, or the event has passed");
            }
                        
            var gymProduct = reservation.GymProduct;
            if (gymProduct.MaxCapacity.HasValue)
            {
                var confirmedReservationsSpec = new ReservationsBySlotSpec(reservation.GymProductId, reservation.ReservationDateTime);
                var existingReservations = await _reservationRepository.ListAsync(confirmedReservationsSpec, cancellationToken);
                var actualConfirmedCount = existingReservations.Count(r => r.Status == ReservationStatus.Confirmed);

                if (actualConfirmedCount >= gymProduct.MaxCapacity.Value)
                {
                    return Error.Validation("Reservation.CapacityExceeded",
                        $"Cannot confirm reservation. Maximum capacity ({gymProduct.MaxCapacity}) has been reached");
                }
            }
                        
            try
            {
                reservation.Confirm(currentTime);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ReservationMapper.ToDto(reservation, gymProduct);
            }
            catch (InvalidOperationException ex)
            {
                return Error.Validation("Reservation.ConfirmationError", ex.Message);
            }
        }
    }
}
