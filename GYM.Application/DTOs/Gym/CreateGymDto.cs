using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Gym;

public sealed record CreateGymDto : ICreateDto
{
    [Required]
    [MinLength(Domain.Entities.Gym.Constants.NameMinLength, ErrorMessage = "Name is too short.")]
    [MaxLength(Domain.Entities.Gym.Constants.NameMaxLength, ErrorMessage = "Name is too long.")]
    public required string Name { get; init; }
}
