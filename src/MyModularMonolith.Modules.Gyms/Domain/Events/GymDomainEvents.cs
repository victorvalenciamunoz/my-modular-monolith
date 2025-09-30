using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Domain.Events;

internal record GymCreatedDomainEvent(
Guid GymId,
string Name,
DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}

internal record GymUpdatedDomainEvent(
Guid GymId,
string Name,
DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}

