using GYM.Application.DTOs.Member;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IMemberService :
    IService<Member, MemberResponseDto, CreateMemberDto, MemberUpdateDto>
{
}
