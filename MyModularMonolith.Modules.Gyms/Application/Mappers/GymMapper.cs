using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;

namespace MyModularMonolith.Modules.Gyms.Application.Mappers;

internal static class GymMapper
{
    public static GymDto ToDto(Gym gym) =>
        new(
            gym.Id,
            gym.Name,
            gym.IsActive,
            gym.CreatedAt,
            gym.UpdatedAt);
}
