using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Domain;

namespace MyModularMonolith.Modules.Gyms.Application.Mappers;

internal static class ReservationMapper
{
    public static ReservationDto ToDto(Reservation reservation, GymProduct gymProduct) =>
        new(
            reservation.Id,
            reservation.UserId,
            reservation.GymProductId,
            gymProduct.Gym.Name,
            gymProduct.Product.Name,
            reservation.ReservationDateTime,
            reservation.Status.ToString(),
            gymProduct.Price,
            gymProduct.DiscountPercentage,
            gymProduct.GetFinalPrice(),
            reservation.UserNotes,
            reservation.CancellationReason,
            reservation.CancelledAt,
            reservation.CreatedAt);
}