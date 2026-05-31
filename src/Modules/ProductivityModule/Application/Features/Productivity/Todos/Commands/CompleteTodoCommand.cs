using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Todos.Commands;

/// <summary>
/// Marks a todo as complete or incomplete.
/// </summary>
public sealed record CompleteTodoCommand(Guid TodoId, bool IsComplete) : IRequest<Result>;
/// <summary>
/// Complete todo command handler.
/// </summary>

public sealed class CompleteTodoCommandHandler(
    ITodoRepository todoRepository,
    IRepository<Reminder> reminderRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CompleteTodoCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.TodoId, cancellationToken);
        if (todo is null)
        {
            return Result.Failure("Todo was not found.");
        }

        if (request.IsComplete)
        {
            todo.Complete();
            var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
            await notificationService.SendReminderAsync(userId, "Todo completed", $"Todo '{todo.Title}' was completed.", ["InApp"], cancellationToken);

            var reminders = await reminderRepository.FindAsync(
                new SimpleReminderSpecification(todo.Id),
                cancellationToken);

            foreach (var reminder in reminders)
            {
                reminder.MarkCompleted();
                reminderRepository.Update(reminder);
            }
        }
        else
        {
            todo.Uncomplete();
        }

        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
    /// <summary>
    /// Simple reminder specification.
    /// </summary>

    private sealed class SimpleReminderSpecification(Guid todoId) : Alphabet.Domain.Specifications.BaseSpecification<Reminder>(x => x.LinkedEntityId == todoId)
    {
    }
}
