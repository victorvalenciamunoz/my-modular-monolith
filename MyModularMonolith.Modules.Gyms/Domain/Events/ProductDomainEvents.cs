using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Domain.Events;

public record ProductCreatedDomainEvent(
    Guid ProductId,
    string ProductName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}

public record ProductUpdatedDomainEvent(
    Guid ProductId,
    string ProductName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}

public record ProductActivatedDomainEvent(
    Guid ProductId,
    string ProductName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}

public record ProductDeactivatedDomainEvent(
    Guid ProductId,
    string ProductName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id => Guid.NewGuid();
}
