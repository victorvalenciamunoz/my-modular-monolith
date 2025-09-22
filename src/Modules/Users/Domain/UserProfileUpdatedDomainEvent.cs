using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Users.Domain;

internal record UserProfileUpdatedDomainEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}