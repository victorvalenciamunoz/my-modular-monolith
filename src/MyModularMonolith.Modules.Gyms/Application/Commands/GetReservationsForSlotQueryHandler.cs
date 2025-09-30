using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class GetReservationsForSlotQueryHandler : IRequestHandler<GetReservationsForSlotQuery, ErrorOr<SlotReservationsDto>>
{
    private readonly IGymProductRepository _gymProductRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsForSlotQueryHandler(
        IGymProductRepository gymProductRepository,
        IReservationRepository reservationRepository)
    {
        _gymProductRepository = gymProductRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<ErrorOr<SlotReservationsDto>> Handle(GetReservationsForSlotQuery request, CancellationToken cancellationToken)
    {
        var gymProductSpec = new GetGymProductByIdWithNavigationSpec(request.GymProductId);
        var gymProduct = await _gymProductRepository.FirstOrDefaultAsync(gymProductSpec, cancellationToken);

        if (gymProduct == null)
        {
            return Error.NotFound("GymProduct.NotFound",
                $"Gym product with ID {request.GymProductId} was not found");
        }

        bool isValidSlot = true;
        string timeSlotString = "No schedule required";

        if (gymProduct.Product.RequiresSchedule)
        {
            isValidSlot = gymProduct.IsValidReservationTime(request.SlotDateTime);
            if (isValidSlot)
            {
                var availableSlots = gymProduct.GetAvailableTimeSlotsForDate(request.SlotDateTime.Date);
                var requestedTimeOnly = request.SlotDateTime.TimeOfDay;
                var matchingSlot = availableSlots.FirstOrDefault(slot =>
                    slot.StartTime <= requestedTimeOnly && requestedTimeOnly < slot.EndTime);
                timeSlotString = matchingSlot?.ToString() ?? $"{request.SlotDateTime:HH:mm}";
            }
            else
            {
                timeSlotString = $"{request.SlotDateTime:HH:mm} (Invalid)";
            }
        }

        var reservationsSpec = new ReservationsBySlotSpec(request.GymProductId, request.SlotDateTime);
        var reservations = await _reservationRepository.ListAsync(reservationsSpec, cancellationToken);

        var totalReservations = reservations.Count;
        var pendingReservations = reservations.Count(r => r.Status == ReservationStatus.Pending);
        var confirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed);
        var isOverbooked = gymProduct.MaxCapacity.HasValue && totalReservations > gymProduct.MaxCapacity.Value;

        var reservationDtos = reservations
            .OrderBy(r => r.Status == ReservationStatus.Pending ? 0 : 1)
            .ThenBy(r => r.CreatedAt)
            .Select(r => new SlotReservationDto(
                r.Id,
                r.UserId,
                r.ReservationDateTime,
                r.Status.ToString(),
                r.UserNotes,
                r.CreatedAt
            )).ToList();

        return new SlotReservationsDto(
            request.GymProductId,
            gymProduct.Gym.Name,
            gymProduct.Product.Name,
            request.SlotDateTime,
            timeSlotString,
            isValidSlot,
            totalReservations,
            pendingReservations,
            confirmedReservations,
            gymProduct.MaxCapacity,
            isOverbooked,
            reservationDtos);
    }
}
