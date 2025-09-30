using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Gyms.Infrastructure;

internal class GymProductRepository : EfRepository<GymProduct>, IGymProductRepository
{
    public GymProductRepository(GymsDbContext context) : base(context)
    {
    }
}
