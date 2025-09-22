using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Shared.Infrastructure;

namespace MyModularMonolith.Modules.Gyms.Infrastructure;

internal class ReservationRepository : EfRepository<Reservation>, IReservationRepository
{
    private readonly GymsDbContext _context;

    public ReservationRepository(GymsDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<int> GetReservationCountForSlotAsync(Guid gymProductId, DateTime reservationDateTime, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Reservation>()
            .CountAsync(r => r.GymProductId == gymProductId
                          && r.ReservationDateTime == reservationDateTime
                          && r.Status == ReservationStatus.Confirmed, cancellationToken);
    }
}