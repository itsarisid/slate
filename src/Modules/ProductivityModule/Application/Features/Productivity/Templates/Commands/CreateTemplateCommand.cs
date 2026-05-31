using System.Text.Json;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Templates.Commands;

/// <summary>
/// Creates a reusable productivity template.
/// </summary>
public sealed record CreateTemplateCommand(string Name, string EntityType, string? Description, JsonDocument Template) : IRequest<Result<Guid>>;
/// <summary>
/// Create template command handler.
/// </summary>

public sealed class CreateTemplateCommandHandler(
    IRepository<ProductivityTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateTemplateCommand, Result<Guid>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<Guid>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var template = ProductivityTemplate.Create(userId, request.Name, request.EntityType, request.Description, request.Template.RootElement.GetRawText());
        await templateRepository.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
