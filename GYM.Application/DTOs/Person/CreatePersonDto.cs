using GYM.Application.Abstractions;
using GYM.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Person;

public sealed record CreatePersonDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "GymId must be a positive integer.")]
    public required int GymId { get; init; }

    [Required]
    [MaxLength(Domain.Entities.Person.Constants.FullNameMaxLength, ErrorMessage = "Full name is too long.")]
    [MinLength(Domain.Entities.Person.Constants.FullNameMinLength, ErrorMessage = "Full name is too short.")]
    public required string FullName { get; init; }

    [Required]
    [MaxLength(Domain.Entities.Person.Constants.PhoneMaxLength, ErrorMessage = "Phone is too long.")]
    [MinLength(Domain.Entities.Person.Constants.PhoneMinLength, ErrorMessage = "Phone is too short.")]
    public required string Phone { get; init; }

    [MaxLength(Domain.Entities.Person.Constants.EmailMaxLength, ErrorMessage = "Email is too long.")]
    [EmailAddress]
    public string? Email { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Required]
    [EnumDataType(typeof(Gender))]
    public required Gender Gender { get; init; }

    [MaxLength(Domain.Entities.Person.Constants.AddressMaxLength, ErrorMessage = "Address is too long.")]
    public string? Address { get; init; }
}