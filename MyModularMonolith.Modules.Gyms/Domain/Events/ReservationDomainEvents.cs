using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Domain.Events;

public record ReservationCancelledDomainEvent(
    Guid ReservationId,
    string UserId,
    string CancellationReason,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}

public record ReservationCompletedDomainEvent(
    Guid ReservationId,
    string UserId,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}

public record ReservationCreatedDomainEvent(
    Guid ReservationId,
    string UserId,
    Guid GymProductId,
    DateTime ReservationDateTime,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}

public record ReservationConfirmedDomainEvent(
    Guid ReservationId,
    string UserId,
    DateTime ConfirmedAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = ConfirmedAt;
}
