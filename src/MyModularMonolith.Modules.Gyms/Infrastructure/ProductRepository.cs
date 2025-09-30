using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Gyms.Infrastructure;

internal class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(GymsDbContext context) : base(context)
    {
    }
}
