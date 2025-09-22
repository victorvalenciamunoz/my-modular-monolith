using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Gyms.Infrastructure
{
    internal class GymRepository : EfRepository<Gym>, IGymRepository
    {
        public GymRepository(GymsDbContext context) : base(context)
        {
        }
    }
}
