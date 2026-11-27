using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.RefreshToken;

public sealed record RefreshTokenResponseDto(
    int Id,
    int UserId,
    string Token,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    DateTime? RevokedAt,
    bool IsActive)
    : IResponseDto<Domain.Entities.RefreshToken, RefreshTokenResponseDto>
{
    public static RefreshTokenResponseDto FromEntity(Domain.Entities.RefreshToken entity)
    {
        return new RefreshTokenResponseDto(
            Id: entity.Id.Value,
            UserId: entity.UserId.Value,
            Token: entity.Token,
            ExpiresAt: entity.ExpiresAt,
            CreatedAt: entity.CreatedAt,
            RevokedAt: entity.RevokedAt,
            IsActive: entity.IsActive);
    }
}
