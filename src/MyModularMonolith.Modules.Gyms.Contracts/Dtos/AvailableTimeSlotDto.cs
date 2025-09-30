namespace MyModularMonolith.Modules.Gyms.Contracts;

public record AvailableTimeSlotDto(
    string TimeSlot,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int CurrentReservations,
    int? MaxCapacity,
    bool IsAvailable);

public record AvailableTimeSlotsDto(
    Guid GymProductId,
    string GymName,
    string ProductName,
    DateTime Date,
    List<AvailableTimeSlotDto> TimeSlots);