using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Person;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class PersonService
    : ServiceBase<Person, PersonResponseDto, CreatePersonDto, UpdatePersonDto>,
        IPersonService
{
    private readonly IPersonRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PersonService> _logger;

    public PersonService(
        IPersonRepository repo,
        IUnitOfWork unitOfWork,
        ILogger<PersonService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreatePersonDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var gymIdResult = Id.Create(dto.GymId);
            if (gymIdResult.IsFailure)
            {
                _logger.LogWarning("Failed to create Person: Invalid GymId {dto.GymId}", dto.GymId);
                if (gymIdResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(gymIdResult.Error);
            }

            var phoneResult = Phone.Create(dto.Phone);
            if (phoneResult.IsFailure)
            {
                _logger.LogWarning("Failed to create Person: Invalid Phone {dto.Phone}", dto.Phone);
                if (phoneResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(phoneResult.Error);
            }

            Result<Email>? emailResult = null;
            if (dto.Email is not null)
            {
                emailResult = Email.Create(dto.Email);
                if (emailResult.IsFailure)
                {
                    _logger.LogWarning("Failed to create Person: Invalid Email {dto.Email}", dto.Email);
                    if (emailResult.Error is ValidationErrors ve)
                        errors.AddRange(ve.Errors);
                    else
                        errors.Add(emailResult.Error);
                }
            }

            Result<DateOfBirth>? dateOfBirthResult = null;
            if (dto.DateOfBirth is not null)
            {
                dateOfBirthResult = DateOfBirth.Create(dto.DateOfBirth.Value);
                if (dateOfBirthResult.IsFailure)
                {
                    _logger.LogWarning("Failed to create Person: Invalid Date of Birth {dto.DateOfBirth}", dto.DateOfBirth);
                    if (dateOfBirthResult.Error is ValidationErrors ve)
                        errors.AddRange(ve.Errors);
                    else
                        errors.Add(dateOfBirthResult.Error);
                }
            }

            var validateResult = Person.Validate(dto.FullName, dto.Address);
            if (validateResult.IsFailure)
            {
                if (validateResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(validateResult.Error);
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("Failed to create Person: Validation errors occurred {Errors}",
                    string.Join(", ", errors.Select(e => e.Message)));
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));
            }

            var personResult = Person.Create(
                gymIdResult.Value,
                dto.FullName,
                phoneResult.Value,
                emailResult?.Value,
                dateOfBirthResult?.Value,
                dto.Gender,
                dto.Address);

            if (personResult.IsFailure)
            {
                _logger.LogWarning("Failed to create Person: Invalid Person data");
                if (personResult.Error is ValidationErrors ve)
                    return Result.Failure<int>(new ValidationErrors(ve.Errors));
                return Result.Failure<int>(new ValidationErrors(new[] { personResult.Error }));
            }

            _repo.Add(personResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(personResult.Value.Id.Value);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogError(e, "An error occurred while creating a Person");

            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int personId,
        UpdatePersonDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(personId);
            if (idResult.IsFailure)
            {
                if (idResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(idResult.Error);
            }

            var phoneResult = Phone.Create(dto.Phone);
            if (phoneResult.IsFailure)
            {
                if (phoneResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(phoneResult.Error);
            }

            Result<Email>? emailResult = null;
            if (dto.Email is not null)
            {
                emailResult = Email.Create(dto.Email);
                if (emailResult.IsFailure)
                {
                    if (emailResult.Error is ValidationErrors ve)
                        errors.AddRange(ve.Errors);
                    else
                        errors.Add(emailResult.Error);
                }
            }

            Result<DateOfBirth>? dateOfBirthResult = null;
            if (dto.DateOfBirth is not null)
            {
                dateOfBirthResult = DateOfBirth.Create(dto.DateOfBirth.Value);
                if (dateOfBirthResult.IsFailure)
                {
                    if (dateOfBirthResult.Error is ValidationErrors ve)
                        errors.AddRange(ve.Errors);
                    else
                        errors.Add(dateOfBirthResult.Error);
                }
            }

            var validateResult = Person.Validate(dto.FullName, dto.Address);
            if (validateResult.IsFailure)
            {
                if (validateResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(validateResult.Error);
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("Failed to update Person: Invalid Person data {errors}", string.Join(", ", errors.Select(e => e.Message)));
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));
            }

            var person = await _repo.GetByIdAsync(
                idResult.Value,
                cancellationToken);
            if (person is null)
            {
                return Result.Failure<int>(
                    ServiceErrors.Common.NotFound);
            }

            var personResult = person.Update(
                dto.FullName,
                phoneResult.Value,
                emailResult?.Value,
                dateOfBirthResult?.Value,
                dto.Gender,
                dto.Address);
            if (personResult.IsFailure)
            {
                if (personResult.Error is ValidationErrors ve)
                    errors.AddRange(ve.Errors);
                else
                    errors.Add(personResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogError(e, "An error occurred while updating a Person");

            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }
}