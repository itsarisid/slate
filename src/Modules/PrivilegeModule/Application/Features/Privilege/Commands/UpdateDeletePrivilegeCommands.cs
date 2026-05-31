using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Privilege.Commands;

/// <summary>
/// Updates privilege metadata.
/// </summary>
public sealed record UpdatePrivilegeCommand(
    Guid PrivilegeId,
    string DisplayName,
    string? Description,
    string Category,
    string? ResourceType,
    IReadOnlyCollection<string> Actions,
    IReadOnlyCollection<string> DependsOn,
    IReadOnlyDictionary<string, string?> Attributes) : IRequest<Result>;

/// <summary>
/// Deprecates or hard-deletes a privilege.
/// </summary>
public sealed record DeletePrivilegeCommand(Guid PrivilegeId, bool HardDelete) : IRequest<Result>;
/// <summary>
/// Update privilege command validator.
/// </summary>

public sealed class UpdatePrivilegeCommandValidator : AbstractValidator<UpdatePrivilegeCommand>
{
    public UpdatePrivilegeCommandValidator()
    {
        RuleFor(x => x.PrivilegeId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Actions).NotEmpty();
    }
}
/// <summary>
/// Update privilege command handler.
/// </summary>

public sealed class UpdatePrivilegeCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<UpdatePrivilegeCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(UpdatePrivilegeCommand request, CancellationToken cancellationToken)
    {
        return privilegeService.UpdatePrivilegeAsync(
            request.PrivilegeId,
            request.DisplayName,
            request.Description,
            request.Category,
            request.ResourceType,
            request.Actions,
            request.DependsOn,
            request.Attributes,
            cancellationToken);
    }
}
/// <summary>
/// Delete privilege command handler.
/// </summary>

public sealed class DeletePrivilegeCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<DeletePrivilegeCommand, Result>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public Task<Result> Handle(DeletePrivilegeCommand request, CancellationToken cancellationToken)
    {
        return privilegeService.DeletePrivilegeAsync(request.PrivilegeId, request.HardDelete, cancellationToken);
    }
}
