using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Tasks.Commands;

/// <summary>
/// Adds a time entry to a task.
/// </summary>
public sealed record AddTimeEntryCommand(Guid TaskId, DateTimeOffset StartTime, DateTimeOffset EndTime, string Description) : IRequest<Result>;
/// <summary>
/// Add time entry command handler.
/// </summary>

public sealed class AddTimeEntryCommandHandler(
    ITaskRepository taskRepository,
    IRepository<TimeEntry> timeEntryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<AddTimeEntryCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(AddTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return Result.Failure("Task was not found.");
        }

        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var entry = TimeEntry.Create(request.TaskId, userId, request.StartTime, request.EndTime, request.Description);
        await timeEntryRepository.AddAsync(entry, cancellationToken);
        task.AddTime(entry.DurationHours);
        taskRepository.Update(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
