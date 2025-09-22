using Ardalis.GuardClauses;
using MyModularMonolith.Modules.Gyms.Domain.ValueObjects;
using MyModularMonolith.Shared.Domain;
using MyModularMonolith.Shared.Domain.ValueObjects;

namespace MyModularMonolith.Modules.Gyms.Domain;

public class GymProduct : BaseEntity
{
    public Guid GymId { get; private set; }
    public Guid ProductId { get; private set; }
    public Money Price { get; private set; } = Money.Zero;
    public decimal? DiscountPercentage { get; private set; }
    public bool IsActive { get; private set; } = true;

    public WeeklySchedule Schedule { get; private set; } = WeeklySchedule.Empty();
    public int? MinCapacity { get; private set; }
    public int? MaxCapacity { get; private set; }

    public string? InstructorName { get; private set; }
    public string? InstructorEmail { get; private set; }
    public string? InstructorPhone { get; private set; }
        
    public string? Notes { get; private set; }
    public string? Equipment { get; private set; } 

    // Navigation properties
    public Gym Gym { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // EF Core constructor
    private GymProduct() : base() { }

    public GymProduct(
        Guid gymId,
        Guid productId,
        Money price,
        decimal? discountPercentage,
        DateTime createdAt) : this()
    {
        GymId = Guard.Against.Default(gymId, nameof(gymId));
        ProductId = Guard.Against.Default(productId, nameof(productId));
        Price = Guard.Against.Null(price, nameof(price));
        DiscountPercentage = ValidateDiscountPercentage(discountPercentage);
        CreatedAt = createdAt;
    }
        
    public GymProduct(
        Guid gymId,
        Guid productId,
        decimal price,
        decimal? discountPercentage,
        DateTime createdAt) : this(gymId, productId, Money.Create(price), discountPercentage, createdAt)
    {
    }

    public void UpdatePricing(Money price, decimal? discountPercentage, DateTime updatedAt)
    {
        Guard.Against.Null(price, nameof(price));
        var validDiscount = ValidateDiscountPercentage(discountPercentage);

        if (Price.Value == price.Value && DiscountPercentage == validDiscount) return;

        Price = price;
        DiscountPercentage = validDiscount;
        UpdatedAt = updatedAt;
    }

    public void SetSchedule(string? scheduleJson, int? minCapacity, int? maxCapacity, DateTime updatedAt)
    {
        var schedule = WeeklySchedule.FromJsonString(scheduleJson);
        SetSchedule(schedule, minCapacity, maxCapacity, updatedAt);
    }

    internal void SetProduct(Product product)
    {
        Product = Guard.Against.Null(product, nameof(product));
    }

    public void SetSchedule(WeeklySchedule schedule, int? minCapacity, int? maxCapacity, DateTime updatedAt)
    {
        Guard.Against.Null(schedule, nameof(schedule));

        if (minCapacity.HasValue && maxCapacity.HasValue)
        {
            Guard.Against.OutOfRange(minCapacity.Value, nameof(minCapacity), 0, maxCapacity.Value);
        }

        ValidateSchedule(schedule);

        Schedule = schedule;
        MinCapacity = minCapacity;
        MaxCapacity = maxCapacity;
        UpdatedAt = updatedAt;
    }

    public void SetInstructor(string? instructorName, string? instructorEmail, string? instructorPhone, DateTime updatedAt)
    {
        InstructorName = instructorName;
        InstructorEmail = instructorEmail;
        InstructorPhone = instructorPhone;
        UpdatedAt = updatedAt;
    }

    public void SetAdditionalInfo(string? notes, string? equipment, DateTime updatedAt)
    {
        Notes = notes;
        Equipment = equipment;
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTime activatedAt)
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = activatedAt;
    }

    public void Deactivate(DateTime deactivatedAt)
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = deactivatedAt;
    }

    public Money GetFinalPrice()
    {
        if (!DiscountPercentage.HasValue)
            return Price;

        return Price.ApplyDiscount(DiscountPercentage.Value);
    }
        

    public Money CalculateDiscountAmount()
    {
        if (!DiscountPercentage.HasValue)
            return Money.Zero;

        var finalPrice = GetFinalPrice();
        return Price.Subtract(finalPrice);
    }

    public Money ApplyTax(decimal taxPercentage)
    {
        var finalPrice = GetFinalPrice();
        return finalPrice.ApplyTax(taxPercentage);
    }

    public bool IsValidReservationTime(DateTime reservationDateTime)
    {        
        if (Product?.RequiresSchedule != true)
            return true;

        return Schedule.IsValidReservationTime(reservationDateTime);
    }

    public List<TimeSlot> GetAvailableTimeSlotsForDate(DateTime date)
    {
        if (Product?.RequiresSchedule != true)
            return new List<TimeSlot>();

        return Schedule.GetTimeSlotsForDay(date.DayOfWeek);
    }

    private static decimal? ValidateDiscountPercentage(decimal? discountPercentage)
    {
        if (!discountPercentage.HasValue) return null;
        
        Guard.Against.OutOfRange(discountPercentage.Value, nameof(discountPercentage), 0, 100);
        return discountPercentage;
    }

    private void ValidateSchedule(WeeklySchedule schedule)
    {        
        if (Product?.RequiresSchedule != true)
            return;
                
        if (schedule.Schedule.Count == 0)
        {
            throw new InvalidOperationException(
                "Schedule is required for products that require scheduling");
        }
                
        foreach (var daySchedule in schedule.Schedule)
        {
            var timeSlots = daySchedule.Value.OrderBy(ts => ts.StartTime).ToList();

            for (int i = 0; i < timeSlots.Count - 1; i++)
            {
                if (timeSlots[i].Overlaps(timeSlots[i + 1]))
                {
                    throw new InvalidOperationException(
                        $"Overlapping time slots found for {daySchedule.Key}: {timeSlots[i]} and {timeSlots[i + 1]}");
                }
            }
        }
    }
}
