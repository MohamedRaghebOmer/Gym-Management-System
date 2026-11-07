using GYM.Application.Abstractions;
using GYM.Application.DTOs.Member;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IMemberService :
    IService<Member, MemberResponseDto, CreateMemberDto, MemberUpdateDto>
{
}
