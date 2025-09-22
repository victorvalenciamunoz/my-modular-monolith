using Ardalis.Specification;

namespace MyModularMonolith.Modules.Gyms.Domain.Specifications;

public sealed class GetGymProductsByGymIdSpec : Specification<GymProduct>
{
    public GetGymProductsByGymIdSpec(Guid gymId, bool includeInactive = false)
    {
        Query.Where(gp => gp.GymId == gymId);

        if (!includeInactive)
        {
            Query.Where(gp => gp.IsActive);
        }

        Query.Include(gp => gp.Product)
             .Include(gp => gp.Gym)
             .OrderBy(gp => gp.Product.Name);
    }
}

public sealed class GetActiveGymProductsByGymIdSpec : Specification<GymProduct>
{
    public GetActiveGymProductsByGymIdSpec(Guid gymId)
    {
        Query.Where(gp => gp.GymId == gymId && gp.IsActive && gp.Product.IsActive)
             .Include(gp => gp.Product)
             .Include(gp => gp.Gym)
             .OrderBy(gp => gp.Product.Name);
    }
}

public sealed class GetGymProductByGymAndProductIdSpec : Specification<GymProduct>
{
    public GetGymProductByGymAndProductIdSpec(Guid gymId, Guid productId)
    {
        Query.Where(gp => gp.GymId == gymId && gp.ProductId == productId)
             .Include(gp => gp.Product)
             .Include(gp => gp.Gym);
    }
}

public sealed class GetGymProductByIdWithNavigationSpec : Specification<GymProduct>
{
    public GetGymProductByIdWithNavigationSpec(Guid gymProductId)
    {
        Query.Where(gp => gp.Id == gymProductId)
             .Include(gp => gp.Product)
             .Include(gp => gp.Gym);
    }
}

public sealed class GetGymProductsWithScheduleSpec : Specification<GymProduct>
{
    public GetGymProductsWithScheduleSpec(Guid? gymId = null)
    {
        Query.Where(gp => gp.IsActive &&
                         gp.Product.IsActive &&
                         gp.Product.RequiresSchedule &&
                         gp.Schedule != null);

        if (gymId.HasValue)
        {
            Query.Where(gp => gp.GymId == gymId.Value);
        }

        Query.Include(gp => gp.Product)
             .Include(gp => gp.Gym)
             .OrderBy(gp => gp.Gym.Name)
             .ThenBy(gp => gp.Product.Name);
    }
}