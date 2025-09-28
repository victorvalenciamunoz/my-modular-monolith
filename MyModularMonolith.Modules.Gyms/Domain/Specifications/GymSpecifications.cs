using Ardalis.Specification;

namespace MyModularMonolith.Modules.Gyms.Domain.Specifications;

public sealed class GetActiveGymsSpec : Specification<Gym>
{
    public GetActiveGymsSpec()
    {
        Query.Where(g => g.IsActive)
             .OrderBy(g => g.Name);
    }
}

public sealed class GetGymByIdSpec : Specification<Gym>
{
    public GetGymByIdSpec(Guid gymId)
    {
        Query.Where(g => g.Id == gymId);
    }
}

public sealed class GetGymByNameSpec : Specification<Gym>
{
    public GetGymByNameSpec(string gymName, bool includeInactiveProducts = false)
    {
        Query.Where(g => g.Name.ToUpper() == gymName.ToUpper())
             .Include(g => g.GymProducts.Where(gp => includeInactiveProducts || gp.IsActive))
             .ThenInclude(gp => gp.Product);
    }
}

public sealed class GetGymsWithProductsSpec : Specification<Gym>
{
    public GetGymsWithProductsSpec(bool includeInactive = false)
    {
        Query.Include(g => g.GymProducts)
             .ThenInclude(gp => gp.Product);

        if (!includeInactive)
        {
            Query.Where(g => g.IsActive);
        }

        Query.OrderBy(g => g.Name);
    }
}

public sealed class GetGymByIdWithProductsSpec : Specification<Gym>
{
    public GetGymByIdWithProductsSpec(Guid gymId, bool includeInactiveProducts = false)
    {
        Query.Where(g => g.Id == gymId)
             .Include(g => g.GymProducts.Where(gp => includeInactiveProducts || gp.IsActive))
             .ThenInclude(gp => gp.Product);
    }
}

