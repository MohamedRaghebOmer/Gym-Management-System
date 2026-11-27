using GYM.Application.Abstractions;
using GYM.Application.DTOs.RefreshToken;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IRefreshTokenService
    : IService<RefreshToken, RefreshTokenResponseDto, CreateRefreshTokenDto, RefreshTokenUpdateDto>
{
}
