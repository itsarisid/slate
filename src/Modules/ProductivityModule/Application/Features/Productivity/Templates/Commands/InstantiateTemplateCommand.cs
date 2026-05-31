using System.Text.Json;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Templates.Commands;

/// <summary>
/// Instantiates a saved template.
/// </summary>
public sealed record InstantiateTemplateCommand(Guid TemplateId) : IRequest<Result<JsonDocument>>;
/// <summary>
/// Instantiate template command handler.
/// </summary>

public sealed class InstantiateTemplateCommandHandler(IRepository<ProductivityTemplate> templateRepository)
    : IRequestHandler<InstantiateTemplateCommand, Result<JsonDocument>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<JsonDocument>> Handle(InstantiateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result<JsonDocument>.Failure("Template was not found.");
        }

        return JsonDocument.Parse(template.TemplateJson);
    }
}
