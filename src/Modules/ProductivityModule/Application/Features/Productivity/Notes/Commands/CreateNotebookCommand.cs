using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Notes.Commands;

/// <summary>
/// Creates a notebook.
/// </summary>
public sealed record CreateNotebookCommand(string Name, string? Description, string? Color, Guid? ParentNotebookId) : IRequest<Result<Guid>>;
/// <summary>
/// Create notebook command handler.
/// </summary>

public sealed class CreateNotebookCommandHandler(
    IRepository<Notebook> notebookRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateNotebookCommand, Result<Guid>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<Guid>> Handle(CreateNotebookCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var notebook = Notebook.Create(userId, request.Name, request.Description, request.Color, request.ParentNotebookId);
        await notebookRepository.AddAsync(notebook, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return notebook.Id;
    }
}
