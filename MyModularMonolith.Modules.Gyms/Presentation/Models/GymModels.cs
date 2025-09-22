using MyModularMonolith.Modules.Gyms.Contracts;

namespace MyModularMonolith.Modules.Gyms.Presentation.Models
{
    public record GymResponse(
        Guid Id,
        string Name,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt)
    {
        public static GymResponse FromDto(GymDto dto)
            => new(dto.Id, dto.Name, dto.IsActive, dto.CreatedAt, dto.UpdatedAt);
    }

    public record GymsListResponse(
        List<GymResponse> Gyms,
        int TotalCount,
        DateTime ResponseTimestamp)
    {
        public static GymsListResponse FromDtos(List<GymDto> dtos)
            => new(
                dtos.Select(GymResponse.FromDto).ToList(),
                dtos.Count,
                DateTime.UtcNow);
    }
}
