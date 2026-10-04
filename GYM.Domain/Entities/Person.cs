using GYM.Domain.Enums;
using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Person : Entity
{
    public static class Constants
    {
        public const int FullNameMaxLength = 100;
        public const int FullNameMinLength = 2;

        public const int PhoneMaxLength = Phone.MaxLength;
        public const int PhoneMinLength = Phone.MinLength;

        public const int EmailMaxLength = Email.MaxLength;
        public const int MaxAge = DateOfBirth.MaxAge;
        public const int AddressMaxLength = 200;
    }

    private Person(
        Id gymId,
        string fullName,
        Phone phone,
        Email? email,
        DateOfBirth? dateOfBirth,
        Gender gender,
        string? address)
    {
        GymId = gymId;
        FullName = fullName;
        Phone = phone;
        Email = email;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }

    private Person() { } // For EF Core


    public Id GymId { get; private set; } = default!;
    public string FullName { get; private set; } = null!;
    public Phone Phone { get; private set; } = default!;
    public Email? Email { get; private set; }
    public DateOfBirth? DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string? Address { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }


    public static Result<Person> Create(
        Id gymId,
        string fullName,
        Phone phone,
        Email? email,
        DateOfBirth? dateOfBirth,
        Gender gender,
        string? address)
    {
        fullName = fullName.Trim();
        address = address?.Trim();

        var result = Validate(fullName, address);
        if (result.IsFailure)
            return Result.Failure<Person>(result.Error);

        return new Person(gymId, fullName, phone, email, dateOfBirth, gender, address);
    }

    public Result Update(
        string fullName,
        Phone phone,
        Email? email,
        DateOfBirth? dateOfBirth,
        Gender gender,
        string? address)
    {
        fullName = fullName.Trim();
        address = address?.Trim();
        var result = Validate(fullName, address);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        FullName = fullName;
        Phone = phone;
        Email = email;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Address = address;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        string fullName,
        string? address)
    {
        List<Error> errors = [];

        // ====================== Full Name ======================
        if (string.IsNullOrWhiteSpace(fullName))
            errors.Add(DomainErrors.Entities.Person.FullName.Empty);

        if (fullName.Length < Constants.FullNameMinLength)
            errors.Add(DomainErrors.Entities.Person.FullName.TooShort);

        if (fullName.Length > Constants.FullNameMaxLength)
            errors.Add(DomainErrors.Entities.Person.FullName.TooLong);


        // ====================== Address ======================
        if (!string.IsNullOrWhiteSpace(address))
        {
            if (address.Length > Constants.AddressMaxLength)
                errors.Add(DomainErrors.Entities.Person.Address.TooLong);
        }

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}