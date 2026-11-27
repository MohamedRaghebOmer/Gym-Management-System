using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.RefreshToken;

public sealed record RefreshTokenUpdateDto : IUpdateDto
{
    [Required]
    [MaxLength(Domain.Entities.RefreshToken.Constants.TokenMaxLength, ErrorMessage = "Token is too long.")]
    public required string Token { get; init; }

    [Required]
    [DataType(DataType.DateTime, ErrorMessage = "ExpiresAt must be a valid date time.")]
    public required DateTime ExpiresAt { get; init; }
}
