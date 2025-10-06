using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Domain;

namespace MyModularMonolith.Modules.Gyms.Application.Services;

public interface IProductAccessService
{
    bool CanUserAccessProduct(MembershipLevel userMembershipLevel, Product product);

    IEnumerable<Product> FilterProductsByMembership(MembershipLevel userMembershipLevel, IEnumerable<Product> products);
}
