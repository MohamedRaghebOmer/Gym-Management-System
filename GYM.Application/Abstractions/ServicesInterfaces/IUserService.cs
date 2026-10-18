using GYM.Application.DTOs.User;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IUserService : IService<User, UserResponseDto, CreateUserDto, UpdateUserDto>
{

}
