using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Todos.Commands;

/// <summary>
/// Creates a new todo item.
/// </summary>
public sealed record CreateTodoCommand(
    string Title,
    string Description,
    Alphabet.Domain.Enums.Priority Priority,
    DateTimeOffset? DueDate,
    int? ReminderMinutesBefore,
    string? Category,
    IReadOnlyCollection<string>? Tags,
    bool IsRecurring,
    RecurrencePattern? RecurrencePattern,
    Guid? AssignedTo) : IRequest<Result<TodoDto>>;
/// <summary>
/// Create todo command validator.
/// </summary>

public sealed class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}
/// <summary>
/// Create todo command handler.
/// </summary>

public sealed class CreateTodoCommandHandler(
    ITodoRepository todoRepository,
    IRepository<Tag> tagRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateTodoCommand, Result<TodoDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<TodoDto>> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var todo = Todo.Create(
            userId,
            request.AssignedTo,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.Category,
            request.ReminderMinutesBefore,
            request.IsRecurring,
            request.RecurrencePattern);

        await todoRepository.AddAsync(todo, cancellationToken);

        foreach (var tag in request.Tags?.Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase) ?? [])
        {
            await tagRepository.AddAsync(Tag.Create("Todo", todo.Id, tag), cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return todo.ToDto();
    }
}
