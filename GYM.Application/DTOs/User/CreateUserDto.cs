using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.User;

public sealed record CreateUserDto : ICreateDto
{
    [Required]
    public required int PersonId { get; init; }

    [Required]
    [MinLength(Domain.Entities.User.Constants.UsernameMinLength, ErrorMessage = "Username is too short.")]
    [MaxLength(Domain.Entities.User.Constants.UsernameMaxLength, ErrorMessage = "Username is too long.")]
    public required string Username { get; init; }

    [Required]
    [MinLength(6, ErrorMessage = "Password is too short.")]
    public required string Password { get; init; }

    [Required]
    public required int RoleId { get; init; }
}
