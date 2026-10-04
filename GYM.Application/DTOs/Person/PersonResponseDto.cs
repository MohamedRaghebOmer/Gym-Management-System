using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Person;

public sealed record PersonResponseDto(
    int GymId,
    string Fullname,
    string Phone,
    string? Email,
    DateOnly? DateOfBirth,
    byte Gender,
    string? Address,
    DateTime CreatedAt)
    : IResponseDto<Domain.Entities.Person, PersonResponseDto>
{
    public static PersonResponseDto FromEntity(Domain.Entities.Person entity)
    {
        return new PersonResponseDto(
            GymId: entity.GymId.Value,
            Fullname: entity.FullName,
            Phone: entity.Phone.Value,
            Email: entity.Email?.Value,
            DateOfBirth: entity.DateOfBirth?.Value,
            Gender: (byte)entity.Gender,
            Address: entity.Address,
            CreatedAt: entity.CreatedAt);
    }
}