using Ardalis.Specification;

namespace MyModularMonolith.Modules.Gyms.Domain;

public interface IReservationRepository : IRepositoryBase<Reservation>
{    
    Task<int> GetReservationCountForSlotAsync(Guid gymProductId, DateTime reservationDateTime, CancellationToken cancellationToken = default);
}