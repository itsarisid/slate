using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Features.Productivity.Reminders.Commands;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Productivity.CrossEntity.Commands;

/// <summary>
/// Creates a reminder from another productivity item.
/// </summary>
public sealed record CreateReminderFromEntityCommand(string EntityType, Guid EntityId, string Title, string Description, DateTimeOffset ReminderTime) : IRequest<Result<ReminderDto>>;
/// <summary>
/// Create reminder from entity command handler.
/// </summary>

public sealed class CreateReminderFromEntityCommandHandler(ISender sender)
    : IRequestHandler<CreateReminderFromEntityCommand, Result<ReminderDto>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<ReminderDto>> Handle(CreateReminderFromEntityCommand request, CancellationToken cancellationToken)
    {
        return await sender.Send(
            new CreateReminderCommand(
                request.Title,
                request.Description,
                request.ReminderTime,
                Alphabet.Domain.Enums.ReminderType.Once,
                null,
                null,
                null,
                true,
                false,
                true,
                10,
                request.EntityType,
                request.EntityId,
                ["Email", "InApp"],
                null),
            cancellationToken);
    }
}
