using Ardalis.Specification;

namespace MyModularMonolith.Modules.Gyms.Domain.Specifications;

public sealed class ActiveReservationByUserSpec : Specification<Reservation>
{
    public ActiveReservationByUserSpec(string userId)
    {
        Query.Where(r => r.UserId == userId && r.Status == ReservationStatus.Confirmed)
             .OrderByDescending(r => r.CreatedAt);
    }
}

public sealed class ReservationsBySlotSpec : Specification<Reservation>
{
    public ReservationsBySlotSpec(Guid gymProductId, DateTime reservationDateTime)
    {
        Query.Where(r => r.GymProductId == gymProductId
                      && r.ReservationDateTime == reservationDateTime);
    }
}

public sealed class UserReservationsSpec : Specification<Reservation>
{
    public UserReservationsSpec(string userId, bool includeCompleted = false)
    {
        Query.Where(r => r.UserId == userId);

        if (!includeCompleted)
        {
            Query.Where(r => r.Status != ReservationStatus.Completed);
        }

        Query.Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Product)
             .Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Gym)
             .OrderByDescending(r => r.CreatedAt); 
    }
}

public sealed class GetReservationByIdWithNavigationSpec : Specification<Reservation>
{
    public GetReservationByIdWithNavigationSpec(Guid reservationId)
    {
        Query.Where(r => r.Id == reservationId)
             .Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Product)
             .Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Gym);
    }
}

public sealed class GetReservationsByGymProductSpec : Specification<Reservation>
{
    public GetReservationsByGymProductSpec(Guid gymProductId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        Query.Where(r => r.GymProductId == gymProductId);

        if (fromDate.HasValue)
            Query.Where(r => r.ReservationDateTime >= fromDate.Value);

        if (toDate.HasValue)
            Query.Where(r => r.ReservationDateTime <= toDate.Value);

        Query.Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Product)
             .Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Gym)
             .OrderBy(r => r.ReservationDateTime)
             .ThenBy(r => r.CreatedAt);
    }
}

public sealed class GetReservationsByGymSpec : Specification<Reservation>
{
    public GetReservationsByGymSpec(Guid gymId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        Query.Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Product)
             .Include(r => r.GymProduct)
             .ThenInclude(gp => gp.Gym)
             .Where(r => r.GymProduct.GymId == gymId);

        if (fromDate.HasValue)
            Query.Where(r => r.ReservationDateTime >= fromDate.Value);

        if (toDate.HasValue)
            Query.Where(r => r.ReservationDateTime <= toDate.Value);

        Query.OrderBy(r => r.ReservationDateTime)
             .ThenBy(r => r.CreatedAt);
    }
    
    public sealed class UserReservationForSlotSpec : Specification<Reservation>
    {
        public UserReservationForSlotSpec(string userId, Guid gymProductId, DateTime reservationDateTime)
        {
            Query.Where(r => r.UserId == userId
                          && r.GymProductId == gymProductId
                          && r.ReservationDateTime == reservationDateTime
                          && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed));
        }
    }
}