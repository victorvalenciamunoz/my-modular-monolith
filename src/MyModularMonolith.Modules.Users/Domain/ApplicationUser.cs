using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using MyModularMonolith.Modules.Users.Domain.Events;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Users.Domain;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; private set; } = string.Empty;
    
    public string LastName { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? UpdatedAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;

    public Guid? HomeGymId { get; private set; }
    
    public string? HomeGymName { get; set; }
    
    public DateTime? RegistrationDate { get; private set; }
    
    public MembershipLevel MembershipLevel { get; private set; } = MembershipLevel.Standard;

    
    public bool HasTemporaryPassword { get; private set; } = false;
    
    public DateTime? TemporaryPasswordCreatedAt { get; private set; }
    
    public bool MustChangePassword { get; private set; } = false;

    private readonly List<IDomainEvent> _domainEvents = [];
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public ApplicationUser() : base()
    {
        Id = Guid.NewGuid();
    }

    public ApplicationUser(string email, 
                            string firstName, 
                            string lastName, 
                            string userName, 
                            DateTime createdAt,
                            Guid? homeGymId = null,
                            string? homeGymName = null,
                            bool hasTemporaryPassword = false) : this()
    {
        Email = Guard.Against.NullOrEmpty(email, nameof(email));
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(firstName));
        LastName = Guard.Against.NullOrEmpty(lastName, nameof(lastName));
        UserName = Guard.Against.NullOrEmpty(userName, nameof(userName));
        CreatedAt = createdAt;
        EmailConfirmed = false;
        HomeGymId = homeGymId;
        HomeGymName = homeGymName;

        HasTemporaryPassword = hasTemporaryPassword;
        TemporaryPasswordCreatedAt = hasTemporaryPassword ? createdAt : null;
        MustChangePassword = hasTemporaryPassword;

        RaiseDomainEvent(new UserRegisteredDomainEvent(Id, 
                                                    Email, FirstName, LastName, 
                                                    createdAt,
                                                    homeGymId, homeGymName, hasTemporaryPassword));
    }

    public void UpdateMembershipLevel(MembershipLevel membershipLevel, DateTime updatedAt)
    {
        if (MembershipLevel == membershipLevel) return;

        var oldMembershipLevel = MembershipLevel;
        MembershipLevel = membershipLevel;
        UpdatedAt = updatedAt;

        RaiseDomainEvent(new UserMembershipLevelChangedDomainEvent(Id,
                                                             oldMembershipLevel,
                                                             membershipLevel,
                                                             updatedAt));
    }

    public void PasswordChanged(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
                
        var hadSpecialCondition = HasTemporaryPassword || MustChangePassword;

        if (hadSpecialCondition)
        {
            HasTemporaryPassword = false;
            MustChangePassword = false;
            TemporaryPasswordCreatedAt = null;

            RaiseDomainEvent(new UserPasswordChangedFromTemporaryDomainEvent(Id, updatedAt));
        }

        
        RaiseDomainEvent(new UserPasswordUpdatedDomainEvent(Id, updatedAt));
    }
    
    public void RequirePasswordChange(DateTime updatedAt)
    {
        MustChangePassword = true;
        UpdatedAt = updatedAt;

        RaiseDomainEvent(new UserPasswordChangeRequiredDomainEvent(Id, updatedAt));
    }

    public bool IsTemporaryPasswordExpired(DateTime currentTime, int expirationDays = 30)
    {
        if (!HasTemporaryPassword || TemporaryPasswordCreatedAt == null)
            return false;

        return currentTime > TemporaryPasswordCreatedAt.Value.AddDays(expirationDays);
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
