using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.ValueObjects;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Tasks.Commands;

/// <summary>
/// Creates a task.
/// </summary>
public sealed record CreateTaskCommand(
    string Title,
    string Description,
    Alphabet.Domain.Enums.Priority Priority,
    Alphabet.Domain.Enums.TaskStatus Status,
    DateTimeOffset? DueDate,
    decimal? EstimatedHours,
    Guid? AssigneeId,
    Guid? ReviewerId,
    Guid? ParentTaskId,
    Guid? ProjectId,
    IReadOnlyCollection<Guid>? Dependencies,
    IReadOnlyCollection<TodoChecklistItem>? Checklist) : IRequest<Result<TaskDto>>;
/// <summary>
/// Create task command handler.
/// </summary>

public sealed class CreateTaskCommandHandler(
    ITaskRepository taskRepository,
    IRepository<TaskDependency> dependencyRepository,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var task = ProductivityTask.Create(
            userId,
            request.Title,
            request.Description,
            request.Priority,
            request.Status,
            request.DueDate,
            request.EstimatedHours,
            request.AssigneeId,
            request.ReviewerId,
            request.ParentTaskId,
            request.ProjectId,
            request.Checklist);

        await taskRepository.AddAsync(task, cancellationToken);
        foreach (var dependencyId in request.Dependencies ?? [])
        {
            await dependencyRepository.AddAsync(TaskDependency.Create(task.Id, dependencyId), cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.AssigneeId.HasValue)
        {
            await notificationService.SendAssignmentAsync(request.AssigneeId.Value, "Task assigned", $"Task '{task.Title}' was assigned to you.", cancellationToken);
        }

        return task.ToDto();
    }
}
