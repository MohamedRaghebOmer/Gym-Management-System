using GYM.Application.Abstractions;
using GYM.Application.DTOs.Person;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IPersonService :
    IService<Person, PersonResponseDto, CreatePersonDto, UpdatePersonDto>
{

}