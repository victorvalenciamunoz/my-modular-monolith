using Ardalis.GuardClauses;
using MyModularMonolith.Shared.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Events;

namespace MyModularMonolith.Modules.Gyms.Domain;

public class Reservation : BaseAggregateRoot
{
    public string UserId { get; private set; } = string.Empty; 
    public Guid GymProductId { get; private set; }
    public DateTime ReservationDateTime { get; private set; } 
    public ReservationStatus Status { get; private set; }
    public string? UserNotes { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    // Navigation properties
    public GymProduct GymProduct { get; private set; } = null!;

    // EF Core constructor
    private Reservation() : base() { }

    public Reservation(
        string userId,
        Guid gymProductId,
        DateTime reservationDateTime,
        string? userNotes,
        DateTime createdAt) : this()
    {
        UserId = Guard.Against.NullOrEmpty(userId, nameof(userId));
        GymProductId = Guard.Against.Default(gymProductId, nameof(gymProductId));
        UserNotes = userNotes;
        Status = ReservationStatus.Pending;
        CreatedAt = createdAt;
                
        if (reservationDateTime <= createdAt)
        {
            throw new InvalidOperationException("Reservation date must be in the future");
        }

        ReservationDateTime = reservationDateTime;

        RaiseDomainEvent(new ReservationCreatedDomainEvent(Id, UserId, GymProductId, ReservationDateTime, createdAt));
    }

    public void ValidateBusinessRules(GymProduct gymProduct, int currentReservationsCount)
    {
        Guard.Against.Null(gymProduct, nameof(gymProduct));
                
        if (gymProduct.Product.RequiresSchedule && !gymProduct.IsValidReservationTime(ReservationDateTime))
        {
            var availableSlots = gymProduct.GetAvailableTimeSlotsForDate(ReservationDateTime.Date);
            var dayName = ReservationDateTime.ToString("dddd", System.Globalization.CultureInfo.CurrentCulture);

            if (availableSlots.Count == 0)
            {
                throw new InvalidOperationException($"No time slots are available for {dayName}");
            }

            var availableSlotsText = string.Join(", ", availableSlots);
            throw new InvalidOperationException(
                $"Invalid reservation time. Available slots for {dayName}: {availableSlotsText}");
        }

    }

    public void Confirm(DateTime confirmedAt)
    {
        if (Status == ReservationStatus.Confirmed)
            return;

        if (Status != ReservationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending reservations can be confirmed");
        }

        Status = ReservationStatus.Confirmed;
        UpdatedAt = confirmedAt;

        RaiseDomainEvent(new ReservationConfirmedDomainEvent(Id, UserId, confirmedAt));
    }

    public void Cancel(string cancellationReason, DateTime cancelledAt)
    {
        if (Status == ReservationStatus.Cancelled)
            return;

        if (Status != ReservationStatus.Pending && Status != ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Only pending or confirmed reservations can be cancelled");
        }

        Status = ReservationStatus.Cancelled;
        CancellationReason = Guard.Against.NullOrEmpty(cancellationReason, nameof(cancellationReason));
        CancelledAt = cancelledAt;
        UpdatedAt = cancelledAt;

        RaiseDomainEvent(new ReservationCancelledDomainEvent(Id, UserId, CancellationReason, cancelledAt));
    }

    public bool CanBeConfirmed(DateTime currentTime)
    {
        return Status == ReservationStatus.Pending &&
               ReservationDateTime > currentTime;
    }

    public void MarkAsCompleted(DateTime completedAt)
    {
        if (Status != ReservationStatus.Confirmed)
            return;

        Status = ReservationStatus.Completed;
        UpdatedAt = completedAt;

        RaiseDomainEvent(new ReservationCompletedDomainEvent(Id, UserId, completedAt));
    }

    public bool CanBeCancelled(DateTime currentTime, int minHoursBeforeReservation = 2)
    {
        return Status == ReservationStatus.Confirmed &&
               ReservationDateTime > currentTime.AddHours(minHoursBeforeReservation);
    }
}

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    NoShow = 4
}