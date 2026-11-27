using GYM.Application.Abstractions;
using GYM.Application.DTOs.RefreshToken;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class RefreshTokenService
    : ServiceBase<RefreshToken, RefreshTokenResponseDto, CreateRefreshTokenDto, RefreshTokenUpdateDto>,
      IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IRefreshTokenRepository repo,
        IUserRepository userRepo,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateRefreshTokenDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var userIdResult = Id.Create(dto.UserId);
            if (userIdResult.IsFailure)
                errors.Add(userIdResult.Error);

            if (userIdResult.IsSuccess)
            {
                var userExists = await _userRepo.ExistsAsync(userIdResult.Value, cancellationToken);
                if (!userExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            var tokenValue = dto.Token.Trim();
            if (!string.IsNullOrWhiteSpace(tokenValue))
            {
                var tokenExists = await _repo.ExistsByTokenAsync(tokenValue, cancellationToken);
                if (tokenExists)
                    errors.Add(ServiceErrors.RefreshToken.Duplicate);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var refreshTokenResult = RefreshToken.Create(
                userIdResult.Value,
                tokenValue,
                dto.ExpiresAt);

            if (refreshTokenResult.IsFailure)
            {
                if (refreshTokenResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(refreshTokenResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(refreshTokenResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(refreshTokenResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a refresh token.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int id,
        RefreshTokenUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var idResult = Id.Create(id);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var refreshToken = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (refreshToken is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var tokenValue = dto.Token.Trim();
            var existingByToken = await _repo.GetByTokenAsync(tokenValue, cancellationToken);
            if (existingByToken is not null && existingByToken.Id != refreshToken.Id)
                return Result.Failure(ServiceErrors.RefreshToken.Duplicate);

            var updateResult = refreshToken.Update(tokenValue, dto.ExpiresAt);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating refresh token {RefreshTokenId}.", id);
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}
