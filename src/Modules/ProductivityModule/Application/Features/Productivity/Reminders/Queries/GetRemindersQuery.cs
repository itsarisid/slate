using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Reminders.Queries;

/// <summary>
/// Gets reminders for the current user.
/// </summary>
public sealed record GetRemindersQuery(
    DateTimeOffset? From,
    DateTimeOffset? To,
    ReminderType? Type,
    ReminderStatus? Status) : IRequest<Result<IReadOnlyList<ReminderDto>>>;
/// <summary>
/// Get reminders query handler.
/// </summary>

public sealed class GetRemindersQueryHandler(
    IRepository<Reminder> reminderRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetRemindersQuery, Result<IReadOnlyList<ReminderDto>>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<IReadOnlyList<ReminderDto>>> Handle(GetRemindersQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var items = await reminderRepository.FindAsync(new ReminderSpecification(userId, request), cancellationToken);
        return Result<IReadOnlyList<ReminderDto>>.Success(items.Select(x => x.ToDto()).ToArray());
    }
    /// <summary>
    /// Reminder specification.
    /// </summary>

    private sealed class ReminderSpecification(Guid userId, GetRemindersQuery query)
        : Alphabet.Domain.Specifications.BaseSpecification<Reminder>(x =>
            x.OwnerUserId == userId &&
            (!query.From.HasValue || x.ReminderTime >= query.From.Value) &&
            (!query.To.HasValue || x.ReminderTime <= query.To.Value) &&
            (!query.Type.HasValue || x.ReminderType == query.Type.Value) &&
            (!query.Status.HasValue || x.Status == query.Status.Value))
    {
    }
}
