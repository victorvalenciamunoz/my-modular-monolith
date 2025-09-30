using Ardalis.GuardClauses;
using MyModularMonolith.Modules.Gyms.Domain.Events;
using MyModularMonolith.Shared.Domain;
using MyModularMonolith.Shared.Domain.ValueObjects;

namespace MyModularMonolith.Modules.Gyms.Domain;

public class Product : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;    
    public Money BasePrice { get; private set; } = Money.Zero;
    public bool RequiresSchedule { get; private set; }
    public bool RequiresInstructor { get; private set; }
    public bool HasCapacityLimits { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation property
    public ICollection<GymProduct> GymProducts { get; private set; } = [];

    // EF Core constructor
    private Product() : base() { }

    public Product(
        string name,
        string description,        
        Money basePrice,
        bool requiresSchedule,
        bool requiresInstructor,
        bool hasCapacityLimits,
        DateTime createdAt) : this()
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = Guard.Against.NullOrEmpty(description, nameof(description));        
        BasePrice = Guard.Against.Null(basePrice, nameof(basePrice));
        RequiresSchedule = requiresSchedule;
        RequiresInstructor = requiresInstructor;
        HasCapacityLimits = hasCapacityLimits;
        CreatedAt = createdAt;

        RaiseDomainEvent(new ProductCreatedDomainEvent(Id, Name, createdAt));
    }

    // Overload constructor for backward compatibility
    public Product(
        string name,
        string description,        
        decimal basePrice,
        bool requiresSchedule,
        bool requiresInstructor,
        bool hasCapacityLimits,
        DateTime createdAt) : this(name, description, Money.Create(basePrice), requiresSchedule, requiresInstructor, hasCapacityLimits, createdAt)
    {
    }

    public void UpdateDetails(
        string name,
        string description,
        Money basePrice,
        bool requiresSchedule,
        bool requiresInstructor,
        bool hasCapacityLimits,
        DateTime updatedAt)
    {
        Guard.Against.NullOrEmpty(name, nameof(name));
        Guard.Against.NullOrEmpty(description, nameof(description));
        Guard.Against.Null(basePrice, nameof(basePrice));

        var hasChanges = Name != name ||
                        Description != description ||
                        BasePrice.Value != basePrice.Value ||
                        RequiresSchedule != requiresSchedule ||
                        RequiresInstructor != requiresInstructor ||
                        HasCapacityLimits != hasCapacityLimits;

        if (!hasChanges) return;

        Name = name;
        Description = description;
        BasePrice = basePrice;
        RequiresSchedule = requiresSchedule;
        RequiresInstructor = requiresInstructor;
        HasCapacityLimits = hasCapacityLimits;
        UpdatedAt = updatedAt;

        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id, Name, updatedAt));
    }

    // Overload for backward compatibility
    public void UpdateDetails(
        string name,
        string description,
        decimal basePrice,
        bool requiresSchedule,
        bool requiresInstructor,
        bool hasCapacityLimits,
        DateTime updatedAt)
        => UpdateDetails(name, description, Money.Create(basePrice), requiresSchedule, requiresInstructor, hasCapacityLimits, updatedAt);

    public void Deactivate(DateTime deactivatedAt)
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = deactivatedAt;

        RaiseDomainEvent(new ProductDeactivatedDomainEvent(Id, Name, deactivatedAt));
    }

    public void Activate(DateTime activatedAt)
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = activatedAt;

        RaiseDomainEvent(new ProductActivatedDomainEvent(Id, Name, activatedAt));
    }

    // Business methods using Money
    public Money CalculatePriceWithMarkup(decimal markupPercentage)
    {
        Guard.Against.Negative(markupPercentage, nameof(markupPercentage));
        return BasePrice.MultiplyBy(1 + markupPercentage / 100);
    }

    public bool IsPriceGreaterThan(Money comparePrice) => BasePrice.IsGreaterThan(comparePrice);
    public bool IsPriceLessThan(Money comparePrice) => BasePrice.IsLessThan(comparePrice);

    // Backward compatibility property
    [Obsolete("Use BasePrice.Value instead")]
    public decimal BasePriceDecimal => BasePrice.Value;
}

