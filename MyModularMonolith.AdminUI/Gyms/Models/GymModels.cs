namespace MyModularMonolith.AdminUI.Gyms.Models
{
    public record GymResponse(Guid Id,
        string Name,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record GymsListResponse(List<GymResponse> Gyms,
        int TotalCount,
        DateTime ResponseTimestamp);
}
