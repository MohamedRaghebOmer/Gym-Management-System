using GYM.Application.DTOs.MembershipPlan;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IMembershipPlanService :
    IService<MembershipPlan, MembershipPlanResponseDto, CreateMembershipPlanDto, MembershipPlanUpdateDto>
{
}
