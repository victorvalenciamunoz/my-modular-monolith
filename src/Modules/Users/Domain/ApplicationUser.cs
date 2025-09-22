using Microsoft.AspNetCore.Identity;
using Ardalis.GuardClauses;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Users.Domain;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public ApplicationUser() : base()
    {
        Id = Guid.NewGuid();
    }

    public ApplicationUser(string email, string firstName, string lastName, string userName, DateTime createdAt) : this()
    {
        Email = Guard.Against.NullOrEmpty(email, nameof(email));
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(firstName));
        LastName = Guard.Against.NullOrEmpty(lastName, nameof(lastName));
        UserName = Guard.Against.NullOrEmpty(userName, nameof(userName));
        CreatedAt = createdAt;
        EmailConfirmed = false;

        RaiseDomainEvent(new UserRegisteredDomainEvent(Id, Email, FirstName, LastName, createdAt));
    }

    public void UpdateProfile(string firstName, string lastName, DateTime updatedAt)
    {
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(firstName));
        LastName = Guard.Against.NullOrEmpty(lastName, nameof(lastName));
        UpdatedAt = updatedAt;

        RaiseDomainEvent(new UserProfileUpdatedDomainEvent(Id, FirstName, LastName, updatedAt));
    }

    public void Activate(DateTime updatedAt)
    {
        if (IsActive) return;
        
        IsActive = true;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTime updatedAt)
    {
        if (!IsActive) return;
        
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    public void ConfirmEmail(DateTime updatedAt)
    {
        if (EmailConfirmed) return;
        
        EmailConfirmed = true;
        UpdatedAt = updatedAt;

        RaiseDomainEvent(new UserEmailConfirmedDomainEvent(Id, Email!, updatedAt));
    }

    private void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}