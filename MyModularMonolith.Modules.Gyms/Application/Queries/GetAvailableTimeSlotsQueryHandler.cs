using ErrorOr;
using MediatR;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Queries;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;

namespace MyModularMonolith.Modules.Gyms.Application.Queries;

internal class GetAvailableTimeSlotsQueryHandler : IRequestHandler<GetAvailableTimeSlotsQuery, ErrorOr<AvailableTimeSlotsDto>>
{
    private readonly IGymProductRepository _gymProductRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetAvailableTimeSlotsQueryHandler(
        IGymProductRepository gymProductRepository,
        IReservationRepository reservationRepository)
    {
        _gymProductRepository = gymProductRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<ErrorOr<AvailableTimeSlotsDto>> Handle(GetAvailableTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        // Obtener el GymProduct con navegaciones
        var gymProductSpec = new GetGymProductByIdWithNavigationSpec(request.GymProductId);
        var gymProduct = await _gymProductRepository.FirstOrDefaultAsync(gymProductSpec, cancellationToken);
        if (gymProduct == null)
        {
            return Error.NotFound("GymProduct.NotFound", $"Gym product with ID {request.GymProductId} was not found");
        }

        // Si el producto no requiere horario, devolver vacío
        if (!gymProduct.Product.RequiresSchedule)
        {
            return new AvailableTimeSlotsDto(
                request.GymProductId,
                gymProduct.Gym.Name,
                gymProduct.Product.Name,
                request.Date,
                new List<AvailableTimeSlotDto>());
        }

        // Obtener slots disponibles para la fecha
        var availableSlots = gymProduct.GetAvailableTimeSlotsForDate(request.Date);
        var timeSlotDtos = new List<AvailableTimeSlotDto>();

        // Para cada slot, verificar cuántas reservas hay
        foreach (var slot in availableSlots)
        {
            var slotDateTime = request.Date.Date.Add(slot.StartTime);
            var currentReservations = await _reservationRepository.GetReservationCountForSlotAsync(
                request.GymProductId, slotDateTime, cancellationToken);

            var startDateTime = request.Date.Date.Add(slot.StartTime);
            var endDateTime = request.Date.Date.Add(slot.EndTime);
            var isAvailable = !gymProduct.MaxCapacity.HasValue || currentReservations < gymProduct.MaxCapacity.Value;

            var dto = new AvailableTimeSlotDto(
                slot.ToString(),
                startDateTime,
                endDateTime,
                currentReservations,
                gymProduct.MaxCapacity,
                isAvailable);

            timeSlotDtos.Add(dto);
        }

        return new AvailableTimeSlotsDto(
            request.GymProductId,
            gymProduct.Gym.Name,
            gymProduct.Product.Name,
            request.Date,
            timeSlotDtos);
    }
}