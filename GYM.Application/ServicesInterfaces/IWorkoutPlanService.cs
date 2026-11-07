using GYM.Application.Abstractions;
using GYM.Application.DTOs.WorkoutPlan;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IWorkoutPlanService :
    IService<WorkoutPlan, WorkoutResponseDto, CreateWorkoutPlanDto, UpdateWorkoutPlanDto>
{

}