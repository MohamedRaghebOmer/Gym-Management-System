using GYM.Application.Abstractions;
using GYM.Application.DTOs.Trainer;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface ITrainerService :
    IService<Trainer, TrainerResponseDto, CreateTrainerDto, TrainerUpdateDto>
{
}
