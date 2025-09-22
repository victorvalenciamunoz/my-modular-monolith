namespace MyModularMonolith.Modules.Gyms.Contracts;

public record SlotReservationDto(
    Guid Id,
    string UserId,
    DateTime ReservationDateTime,
    string Status,
    string? UserNotes,
    DateTime CreatedAt);

public record SlotReservationsDto(
    Guid GymProductId,
    string GymName,
    string ProductName,
    DateTime SlotDateTime,
    string TimeSlotString,
    bool IsValidSlot,
    int TotalReservations,
    int PendingReservations,
    int ConfirmedReservations,
    int? MaxCapacity,
    bool IsOverbooked,
    List<SlotReservationDto> Reservations);
