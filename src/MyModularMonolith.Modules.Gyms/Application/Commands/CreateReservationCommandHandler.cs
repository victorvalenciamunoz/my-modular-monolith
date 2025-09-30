using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Shared.Application;
using static MyModularMonolith.Modules.Gyms.Domain.Specifications.GetReservationsByGymSpec;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ErrorOr<ReservationDto>>
{
    private readonly IGymProductRepository _gymProductRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateReservationCommandHandler(
        IGymProductRepository gymProductRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _gymProductRepository = gymProductRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userReservationForSlotSpec = new UserReservationForSlotSpec(
        request.UserId,
        request.GymProductId,
        request.ReservationDateTime);

        var existingReservation = await _reservationRepository.FirstOrDefaultAsync(userReservationForSlotSpec, cancellationToken);
        if (existingReservation != null)
        {
            return Error.Conflict("Reservation.DuplicateSlotReservation",
                "User already has a reservation for this product and time slot");
        }

        var gymProductSpec = new GetGymProductByIdWithNavigationSpec(request.GymProductId);
        var gymProduct = await _gymProductRepository.FirstOrDefaultAsync(gymProductSpec, cancellationToken);
        if (gymProduct == null)
        {
            return Error.NotFound("GymProduct.NotFound", $"Gym product with ID {request.GymProductId} was not found");
        }

        var slotReservationsSpec = new ReservationsBySlotSpec(request.GymProductId, request.ReservationDateTime);
        var currentReservations = await _reservationRepository.ListAsync(slotReservationsSpec, cancellationToken);
        var currentReservationCount = currentReservations.Count(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed);

        try
        {            
            var reservation = new Reservation(
                request.UserId,
                request.GymProductId,
                request.ReservationDateTime,
                request.UserNotes,
                _dateTimeProvider.UtcNow);
                        
            reservation.ValidateBusinessRules(gymProduct, currentReservationCount);

            await _reservationRepository.AddAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ReservationMapper.ToDto(reservation, gymProduct);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("Reservation.ValidationError", ex.Message);
        }
    }
}