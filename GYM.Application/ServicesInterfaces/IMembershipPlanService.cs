using GYM.Application.Abstractions;
using GYM.Application.DTOs.MembershipPlan;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IMembershipPlanService :
    IService<MembershipPlan, MembershipPlanResponseDto, CreateMembershipPlanDto, MembershipPlanUpdateDto>
{
}
