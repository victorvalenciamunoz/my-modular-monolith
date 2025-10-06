using MyModularMonolith.Modules.Gyms.Application.Services;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Infrastructure.Services;

internal class ProductAccessService : IProductAccessService
{
    public bool CanUserAccessProduct(MembershipLevel userMembershipLevel, Product product)
    {
        return product.IsAccessibleForMembership(userMembershipLevel);
    }

    public IEnumerable<Product> FilterProductsByMembership(MembershipLevel userMembershipLevel, IEnumerable<Product> products)
    {
        return products.Where(p => p.IsActive && p.IsAccessibleForMembership(userMembershipLevel));
    }
}
