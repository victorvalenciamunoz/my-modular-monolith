using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Users.Domain.Events;

internal record UserPasswordChangedFromTemporaryDomainEvent(
Guid UserId,
DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}

internal record UserPasswordChangeRequiredDomainEvent(
Guid UserId,
DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
internal record UserPasswordUpdatedDomainEvent(
    Guid UserId,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}

internal record UserProfileUpdatedDomainEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}

internal record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime OccurredOn,
    Guid? HomeGymId = null,
    string? HomeGymName = null,
    bool HasTemporaryPassword = false) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
internal record UserMembershipLevelChangedDomainEvent(
    Guid UserId,
    MembershipLevel OldMembershipLevel,
    MembershipLevel NewMembershipLevel,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}


