using System.Text.Json;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Productivity.SmartLists.Commands;

/// <summary>
/// Creates a saved smart list.
/// </summary>
public sealed record CreateSmartListCommand(string Name, string EntityType, JsonDocument Criteria, bool IsShared) : IRequest<Result<Guid>>;
/// <summary>
/// Create smart list command handler.
/// </summary>

public sealed class CreateSmartListCommandHandler(
    IRepository<SmartList> smartListRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateSmartListCommand, Result<Guid>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<Guid>> Handle(CreateSmartListCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var list = SmartList.Create(userId, request.Name, request.EntityType, request.Criteria.RootElement.GetRawText(), request.IsShared);
        await smartListRepository.AddAsync(list, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return list.Id;
    }
}
