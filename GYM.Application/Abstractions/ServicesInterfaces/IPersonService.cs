using GYM.Application.DTOs.Person;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IPersonService :
    IService<Person, PersonResponseDto, CreatePersonDto, UpdatePersonDto>
{

}