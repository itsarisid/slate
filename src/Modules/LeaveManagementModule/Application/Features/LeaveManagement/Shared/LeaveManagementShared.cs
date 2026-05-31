using Alphabet.Application.Common.Interfaces;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;

namespace Alphabet.Application.Features.LeaveManagement.Shared;

internal static class LeaveManagementHandlerHelpers
{
    public static Guid RequireUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId ?? throw new InvalidOperationException("An authenticated user is required.");
    }

    public static async Task AddActivityAsync(
        ILeaveRepository repository,
        ICurrentUserService currentUserService,
        Guid? requestId,
        string action,
        string? oldValueJson,
        string? newValueJson,
        string? details,
        CancellationToken cancellationToken)
    {
        await repository.AddActivityAsync(
            LeaveActivityLog.Create(
                currentUserService.UserId,
                requestId,
                action,
                oldValueJson,
                newValueJson,
                currentUserService.IpAddress,
                currentUserService.UserAgent,
                string.IsNullOrWhiteSpace(details) ? null : LeaveManagementJson.Serialize(new { details })),
            cancellationToken);
    }
}
