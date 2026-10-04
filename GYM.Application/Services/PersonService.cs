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
        List<Error> errors = [];

        var gymIdResult = Id.Create(dto.GymId);
        if (gymIdResult.IsFailure)
            errors.Add(gymIdResult.Error);

        var phoneResult = Phone.Create(dto.Phone);
        if (phoneResult.IsFailure)
            errors.Add(phoneResult.Error);

        Result<Email>? emailResult = null;
        if (dto.Email is not null)
        {
            emailResult = Email.Create(dto.Email);
            if (emailResult.IsFailure)
                errors.Add(emailResult.Error);
        }

        Result<DateOfBirth>? dateOfBirthResult = null;
        if (dto.DateOfBirth is not null)
        {
            dateOfBirthResult = DateOfBirth.Create(dto.DateOfBirth.Value);
            if (dateOfBirthResult.IsFailure)
                errors.Add(dateOfBirthResult.Error);
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
            errors.Add(personResult.Error);

        if (errors.Count > 0)
            return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

        _repo.Add(personResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(personResult.Value.Id.Value);
    }

    public override async Task<Result> UpdateAsync(
        int personId,
        UpdatePersonDto dto,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];

        var idResult = Id.Create(personId);
        if (idResult.IsFailure)
            errors.Add(idResult.Error);

        var phoneResult = Phone.Create(dto.Phone);
        if (phoneResult.IsFailure)
            errors.Add(phoneResult.Error);

        Result<Email>? emailResult = null;
        if (dto.Email is not null)
        {
            emailResult = Email.Create(dto.Email);
            if (emailResult.IsFailure)
                errors.Add(emailResult.Error);
        }

        Result<DateOfBirth>? dateOfBirthResult = null;
        if (dto.DateOfBirth is not null)
        {
            dateOfBirthResult = DateOfBirth.Create(dto.DateOfBirth.Value);
            if (dateOfBirthResult.IsFailure)
                errors.Add(dateOfBirthResult.Error);
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
            errors.Add(personResult.Error);

        if (errors.Count > 0)
            return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();

    }
}