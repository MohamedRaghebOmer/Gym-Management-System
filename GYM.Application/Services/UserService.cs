using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.User;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class UserService :
    ServiceBase<User, UserResponseDto, CreateUserDto, UpdateUserDto>,
    IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPersonRepository _personRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository repo,
        IPersonRepository personRepo,
        IRoleRepository roleRepo,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger,
        IPasswordHasher passwordHasher)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _personRepo = personRepo;
        _roleRepo = roleRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            // Validate ids
            var personIdResult = Id.Create(dto.PersonId);
            if (personIdResult.IsFailure)
                errors.Add(personIdResult.Error);

            var roleIdResult = Id.Create(dto.RoleId);
            if (roleIdResult.IsFailure)
                errors.Add(roleIdResult.Error);

            // Validate username/password via domain create later
            // Check existence of person and role
            if (personIdResult.IsSuccess)
            {
                var personExists = await _personRepo.ExistsAsync(personIdResult.Value, cancellationToken);

                if (!personExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            if (roleIdResult.IsSuccess)
            {
                var roleExists = await _roleRepo.ExistsAsync(roleIdResult.Value, cancellationToken);

                if (!roleExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            // Uniqueness checks
            if (personIdResult.IsSuccess)
            {
                var existsByPerson =
                    await _repo.ExistsByPersonIdAsync(
                        personIdResult.Value, cancellationToken);

                if (existsByPerson)
                    errors.Add(ServiceErrors.User.Person.AlreadyAssociatedWithUser);
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                var existsByUsername =
                    await _repo.ExistsByUsernameAsync(
                        dto.Username.Trim(), cancellationToken);

                if (existsByUsername)
                    errors.Add(ServiceErrors.User.Username.AlreadyExists);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            // Domain validations
            var passwordHash = _passwordHasher.Hash(dto.Password);

            var userResult = User.Create(
                personId: personIdResult.Value,
                username: dto.Username,
                passwordHash: passwordHash,
                roleId: roleIdResult.Value);

            if (userResult.IsFailure)
            {
                if (userResult.Error is ValidationErrors validationErrors)
                {
                    errors.AddRange(validationErrors.Errors);
                }
                else
                {
                    errors.Add(userResult.Error);
                }
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(userResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(userResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a user.");
            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int userId,
        UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var idResult = Id.Create(userId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var user = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (user is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            // Check username uniqueness
            var existing = await _repo.GetByUsernameAsync(dto.Username.Trim(), cancellationToken);
            if (existing is not null && existing.Id != user.Id)
                return Result.Failure(ServiceErrors.User.Username.AlreadyExists);

            string passwordHash = _passwordHasher.Hash(dto.Password);

            var updateResult = user.Update(dto.Username, passwordHash, dto.IsActive);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating user {UserId}.", userId);
            return Result.Failure(ServiceErrors.Common.UnExcepected);
        }
    }
}
