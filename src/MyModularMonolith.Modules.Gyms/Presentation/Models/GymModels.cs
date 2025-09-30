using MyModularMonolith.Modules.Gyms.Contracts;
using System.ComponentModel.DataAnnotations;

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

    public record AddGymRequest
    {
        [Required(ErrorMessage = "Gym name is required")]
        [MinLength(2, ErrorMessage = "Gym name must be at least 2 characters long")]
        [MaxLength(100, ErrorMessage = "Gym name must not exceed 100 characters")]
        public required string Name { get; init; }
    }
}
