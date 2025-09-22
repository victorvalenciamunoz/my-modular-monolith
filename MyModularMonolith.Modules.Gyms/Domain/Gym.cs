using Ardalis.GuardClauses;
using MyModularMonolith.Modules.Gyms.Domain.Events;
using MyModularMonolith.Shared.Domain;
using MyModularMonolith.Shared.Domain.ValueObjects;

namespace MyModularMonolith.Modules.Gyms.Domain;

public class Gym : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;    
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public ICollection<GymProduct> GymProducts { get; private set; } = [];

    // EF Core constructor
    private Gym() : base() { }

    public Gym(string name, DateTime createdAt) : this()
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        CreatedAt = createdAt;

        RaiseDomainEvent(new GymCreatedDomainEvent(Id, Name, createdAt));
    }

    public void UpdateName(string name, DateTime updatedAt)
    {
        Guard.Against.NullOrEmpty(name, nameof(name));

        if (Name == name) return;

        Name = name;
        UpdatedAt = updatedAt;

        RaiseDomainEvent(new GymUpdatedDomainEvent(Id, Name, updatedAt));
    }

    public void AddProduct(
        Guid productId, 
        Money price, 
        decimal? discountPercentage, 
        DateTime createdAt)
    {        
        if (GymProducts.Any(gp => gp.ProductId == productId && gp.IsActive))
        {
            throw new InvalidOperationException($"Product {productId} is already active for this gym");
        }

        var gymProduct = new GymProduct(Id, productId, price, discountPercentage, createdAt);
        GymProducts.Add(gymProduct);
    }

    public void RemoveProduct(Guid productId, DateTime deactivatedAt)
    {
        var gymProduct = GymProducts.FirstOrDefault(gp => gp.ProductId == productId && gp.IsActive);
        if (gymProduct == null)
        {
            throw new InvalidOperationException($"Active product {productId} not found for this gym");
        }

        gymProduct.Deactivate(deactivatedAt);
    }

    public IEnumerable<GymProduct> GetActiveProducts() => 
        GymProducts.Where(gp => gp.IsActive);
        
    public Money GetTotalPriceForProducts(IEnumerable<Guid> productIds)
    {
        var total = Money.Zero;
        foreach (var productId in productIds)
        {
            var gymProduct = GymProducts.FirstOrDefault(gp => gp.ProductId == productId && gp.IsActive);
            if (gymProduct != null)
            {
                total = total.Add(gymProduct.GetFinalPrice());
            }
        }
        return total;
    }

    public Money GetAverageProductPrice()
    {
        var activeProducts = GetActiveProducts().ToList();
        if (!activeProducts.Any())
            return Money.Zero;

        var total = activeProducts.Aggregate(Money.Zero, (sum, product) => sum.Add(product.GetFinalPrice()));
        return total.DivideBy(activeProducts.Count);
    }
}
