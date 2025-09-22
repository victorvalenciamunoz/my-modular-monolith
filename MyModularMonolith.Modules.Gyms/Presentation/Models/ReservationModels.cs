using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.Modules.Gyms.Presentation.Models;

public record ReservationResponse(
    Guid Id,
    string UserId,
    Guid GymProductId,
    string GymName,
    string ProductName,
    DateTime ReservationDateTime,
    string Status,
    decimal Price,
    decimal? DiscountPercentage,
    decimal FinalPrice,
    string? UserNotes,
    string? CancellationReason,
    DateTime? CancelledAt,
    DateTime CreatedAt)
{
    public static ReservationResponse FromDto(ReservationDto dto)
        => new(dto.Id, dto.UserId, dto.GymProductId, dto.GymName, dto.ProductName,
               dto.ReservationDateTime, dto.Status, dto.Price, dto.DiscountPercentage,
               dto.FinalPrice, dto.UserNotes, dto.CancellationReason, dto.CancelledAt, dto.CreatedAt);
}

public record UserReservationsResponse(
    List<ReservationResponse> Reservations,
    int TotalCount,
    DateTime ResponseTimestamp)
{
    public static UserReservationsResponse FromDtos(List<ReservationDto> dtos)
        => new(
            dtos.Select(ReservationResponse.FromDto).ToList(),
            dtos.Count,
            DateTime.UtcNow);
}

public record CreateReservationRequest(
    string UserId,
    Guid GymProductId,
    DateTime ReservationDateTime,
    string? UserNotes = null);

public record CancelReservationRequest(
    string UserId,
    string CancellationReason);

public record AvailableTimeSlotResponse(
    string TimeSlot,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int CurrentReservations,
    int? MaxCapacity,
    bool IsAvailable)
{
    public static AvailableTimeSlotResponse FromDto(AvailableTimeSlotDto dto)
        => new(dto.TimeSlot, dto.StartDateTime, dto.EndDateTime,
               dto.CurrentReservations, dto.MaxCapacity, dto.IsAvailable);
}

public record AvailableTimeSlotsResponse(
    Guid GymProductId,
    string GymName,
    string ProductName,
    DateTime Date,
    List<AvailableTimeSlotResponse> TimeSlots,
    DateTime ResponseTimestamp)
{
    public static AvailableTimeSlotsResponse FromDto(AvailableTimeSlotsDto dto)
        => new(dto.GymProductId, dto.GymName, dto.ProductName, dto.Date,
               dto.TimeSlots.Select(AvailableTimeSlotResponse.FromDto).ToList(),
               DateTime.UtcNow);
}

public record SlotReservationResponse(
    Guid Id,
    string UserId,
    DateTime ReservationDateTime,
    string Status,
    string? UserNotes,
    DateTime CreatedAt)
{
    public static SlotReservationResponse FromDto(SlotReservationDto dto) =>
        new(dto.Id, dto.UserId, dto.ReservationDateTime, dto.Status, dto.UserNotes, dto.CreatedAt);
}

public record SlotReservationsResponse(
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
    List<SlotReservationResponse> Reservations,
    DateTime ResponseTimestamp)
{
    public static SlotReservationsResponse FromDto(SlotReservationsDto dto) =>
        new(dto.GymProductId, dto.GymName, dto.ProductName, dto.SlotDateTime,
            dto.TimeSlotString, dto.IsValidSlot, dto.TotalReservations,
            dto.PendingReservations, dto.ConfirmedReservations, dto.MaxCapacity, dto.IsOverbooked,
            dto.Reservations.Select(SlotReservationResponse.FromDto).ToList(),
            DateTime.UtcNow);
}