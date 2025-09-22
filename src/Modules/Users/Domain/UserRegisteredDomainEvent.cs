using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Users.Domain;

internal record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}