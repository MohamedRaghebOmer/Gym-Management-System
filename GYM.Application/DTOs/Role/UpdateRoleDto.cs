using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Role;

public sealed record UpdateRoleDto : IUpdateDto
{
    [Required]
    [MinLength(Domain.Entities.Role.Constants.NameMinLength, ErrorMessage = "Name is too short.")]
    [MaxLength(Domain.Entities.Role.Constants.NameMaxLength, ErrorMessage = "Name is too long.")]
    public required string Name { get; init; }

    [MaxLength(Domain.Entities.Role.Constants.DescriptionMaxLength, ErrorMessage = "Description is too long.")]
    public string? Description { get; init; }
}
