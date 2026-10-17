using GYM.Application.DTOs.Gym;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IGymService :
    IService<Gym, GymResponseDto, CreateGymDto, UpdateGymDto>
{
}
