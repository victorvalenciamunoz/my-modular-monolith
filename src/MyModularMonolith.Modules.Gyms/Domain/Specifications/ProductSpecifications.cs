using Ardalis.Specification;

namespace MyModularMonolith.Modules.Gyms.Domain.Specifications;

public sealed class GetActiveProductsSpec : Specification<Product>
{
    public GetActiveProductsSpec()
    {
        Query.Where(p => p.IsActive)
             .OrderBy(p => p.Name);
    }
}


public sealed class GetProductByIdSpec : Specification<Product>
{
    public GetProductByIdSpec(Guid productId)
    {
        Query.Where(p => p.Id == productId);
    }
}

public sealed class GetProductsWithGymProductsSpec : Specification<Product>
{
    public GetProductsWithGymProductsSpec(bool includeInactive = false)
    {
        Query.Include(p => p.GymProducts)
             .ThenInclude(gp => gp.Gym);

        if (!includeInactive)
        {
            Query.Where(p => p.IsActive);
        }

        Query.OrderBy(p => p.Name);
    }
}
