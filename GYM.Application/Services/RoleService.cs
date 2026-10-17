using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Role;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class RoleService :
    ServiceBase<Role, RoleResponseDto, CreateRoleDto, UpdateRoleDto>,
    IRoleService
{
    private readonly IRoleRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRoleRepository repo,
        IUnitOfWork unitOfWork,
        ILogger<RoleService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var roleResult = Role.Create(dto.Name, dto.Description);
            if (roleResult.IsFailure)
            {
                if (roleResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(roleResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(roleResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(roleResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a role.");
            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int roleId,
        UpdateRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(roleId);
            if (idResult.IsFailure)
            {
                if (idResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(idResult.Error);
            }

            var role = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (role is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var roleResult = role.Update(dto.Name, dto.Description);
            if (roleResult.IsFailure)
            {
                if (roleResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(roleResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating role {RoleId}.", roleId);
            return Result.Failure(ServiceErrors.Common.UnExcepected);
        }
    }
}
