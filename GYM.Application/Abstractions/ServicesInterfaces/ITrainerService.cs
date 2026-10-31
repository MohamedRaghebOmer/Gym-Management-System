using GYM.Application.DTOs.Trainer;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface ITrainerService :
    IService<Trainer, TrainerResponseDto, CreateTrainerDto, TrainerUpdateDto>
{
}
