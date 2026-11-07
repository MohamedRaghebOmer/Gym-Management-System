using GYM.Application.Abstractions;
using GYM.Application.DTOs.Attendance;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class AttendanceService :
    ServiceBase<Attendance, AttendanceResponseDto, CreateAttendanceDto, AttendanceUpdateDto>,
    IAttendanceService
{
    private readonly IAttendanceRepository _repo;
    private readonly IMemberRepository _memberRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository repo,
        IMemberRepository memberRepo,
        ISubscriptionRepository subscriptionRepo,
        IUnitOfWork unitOfWork,
        ILogger<AttendanceService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _memberRepo = memberRepo;
        _subscriptionRepo = subscriptionRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateAttendanceDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var memberIdResult = Id.Create(dto.MemberId);
            if (memberIdResult.IsFailure)
                errors.Add(memberIdResult.Error);

            if (memberIdResult.IsSuccess)
            {
                var memberExists = await _memberRepo.ExistsAsync(memberIdResult.Value, cancellationToken);
                if (!memberExists)
                    errors.Add(ServiceErrors.Common.NotFound);

                var hasActiveSubscription = await _subscriptionRepo.HasActiveSubscriptionAtAsync(
                    memberIdResult.Value,
                    DateOnly.FromDateTime(dto.CheckIn),
                    cancellationToken);

                if (!hasActiveSubscription)
                    errors.Add(ServiceErrors.Attendance.Member.NoActiveSubscription);

                if (dto.CheckOut is null)
                {
                    var hasOpenAttendance = await _repo.HasOpenAttendanceAsync(
                        memberIdResult.Value,
                        null,
                        cancellationToken);

                    if (hasOpenAttendance)
                        errors.Add(ServiceErrors.Attendance.Member.OpenAttendanceAlreadyExists);
                }
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var attendanceResult = Attendance.Create(
                memberIdResult.Value,
                dto.CheckIn,
                dto.CheckOut);

            if (attendanceResult.IsFailure)
            {
                if (attendanceResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(attendanceResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(attendanceResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(attendanceResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating an attendance record.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int attendanceId,
        AttendanceUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(attendanceId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var attendance = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (attendance is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var hasActiveSubscription = await _subscriptionRepo.HasActiveSubscriptionAtAsync(
                attendance.MemberId,
                DateOnly.FromDateTime(dto.CheckIn),
                cancellationToken);

            if (!hasActiveSubscription)
                errors.Add(ServiceErrors.Attendance.Member.NoActiveSubscription);

            if (dto.CheckOut is null)
            {
                var hasOpenAttendance = await _repo.HasOpenAttendanceAsync(
                    attendance.MemberId,
                    attendance.Id,
                    cancellationToken);

                if (hasOpenAttendance)
                    errors.Add(ServiceErrors.Attendance.Member.OpenAttendanceAlreadyExists);
            }

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            var updateResult = attendance.Update(
                dto.CheckIn,
                dto.CheckOut);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating attendance record {AttendanceId}.", attendanceId);
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}
