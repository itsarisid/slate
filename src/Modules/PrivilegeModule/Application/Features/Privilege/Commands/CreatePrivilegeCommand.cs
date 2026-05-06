using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Privilege.Commands;

/// <summary>
/// Creates a new privilege definition.
/// </summary>
public sealed record CreatePrivilegeCommand(
    string Name,
    string DisplayName,
    string? Description,
    string Category,
    string? ResourceType,
    IReadOnlyCollection<string> Actions,
    bool IsGlobal,
    IReadOnlyCollection<string> DependsOn,
    IReadOnlyDictionary<string, string?> Attributes) : IRequest<Result<Guid>>;

/// <summary>
/// Validates privilege creation requests.
/// </summary>
public sealed class CreatePrivilegeCommandValidator : AbstractValidator<CreatePrivilegeCommand>
{
    public CreatePrivilegeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches("^[a-z0-9]+(\\.[a-z0-9]+)+$")
            .WithMessage("Privilege name must follow the resource.action format.");

        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Actions).NotEmpty();
    }
}

/// <summary>
/// Handles privilege creation commands.
/// </summary>
public sealed class CreatePrivilegeCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<CreatePrivilegeCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePrivilegeCommand request, CancellationToken cancellationToken)
    {
        return privilegeService.CreatePrivilegeAsync(
            request.Name,
            request.DisplayName,
            request.Description,
            request.Category,
            request.ResourceType,
            request.Actions,
            request.IsGlobal,
            request.DependsOn,
            request.Attributes,
            cancellationToken);
    }
}
