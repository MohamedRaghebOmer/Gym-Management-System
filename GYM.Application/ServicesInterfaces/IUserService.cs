using GYM.Application.Abstractions;
using GYM.Application.DTOs.User;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IUserService : IService<User, UserResponseDto, CreateUserDto, UpdateUserDto>
{

}
