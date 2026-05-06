using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Privilege.Commands;

public sealed record AssignPrivilegeToRoleCommand(Guid RoleId, IReadOnlyCollection<Guid> PrivilegeIds, DateTimeOffset? ExpiresAt) : IRequest<Result>;

public sealed record RevokePrivilegeFromRoleCommand(Guid RoleId, Guid PrivilegeId) : IRequest<Result>;

public sealed record BulkAssignPrivilegesCommand(
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<Guid> PrivilegeIds,
    string Operation,
    DateTimeOffset? ExpiresAt) : IRequest<Result>;

public sealed record AssignPrivilegeToUserCommand(
    Guid UserId,
    string PrivilegeIdOrName,
    PrivilegeEffect Effect,
    DateTimeOffset? ExpiresAt,
    string? Reason) : IRequest<Result>;

public sealed record RevokePrivilegeFromUserCommand(Guid UserId, Guid PrivilegeId) : IRequest<Result>;

public sealed record AssignPolicyToRoleCommand(Guid RoleId, Guid PolicyId, DateTimeOffset? ExpiresAt) : IRequest<Result>;

public sealed record AssignPolicyToUserCommand(Guid UserId, Guid PolicyId, DateTimeOffset? ExpiresAt) : IRequest<Result>;

public sealed class AssignPrivilegeToRoleCommandValidator : AbstractValidator<AssignPrivilegeToRoleCommand>
{
    public AssignPrivilegeToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PrivilegeIds).NotEmpty();
    }
}

public sealed class AssignPrivilegeToUserCommandValidator : AbstractValidator<AssignPrivilegeToUserCommand>
{
    public AssignPrivilegeToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PrivilegeIdOrName).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(1000);
    }
}

public sealed class AssignPrivilegeToRoleCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<AssignPrivilegeToRoleCommand, Result>
{
    public Task<Result> Handle(AssignPrivilegeToRoleCommand request, CancellationToken cancellationToken)
        => privilegeService.AssignPrivilegesToRoleAsync(request.RoleId, request.PrivilegeIds, request.ExpiresAt, cancellationToken);
}

public sealed class RevokePrivilegeFromRoleCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<RevokePrivilegeFromRoleCommand, Result>
{
    public Task<Result> Handle(RevokePrivilegeFromRoleCommand request, CancellationToken cancellationToken)
        => privilegeService.RevokePrivilegeFromRoleAsync(request.RoleId, request.PrivilegeId, cancellationToken);
}

public sealed class BulkAssignPrivilegesCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<BulkAssignPrivilegesCommand, Result>
{
    public Task<Result> Handle(BulkAssignPrivilegesCommand request, CancellationToken cancellationToken)
        => privilegeService.BulkAssignPrivilegesToRolesAsync(request.RoleIds, request.PrivilegeIds, request.Operation, request.ExpiresAt, cancellationToken);
}

public sealed class AssignPrivilegeToUserCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<AssignPrivilegeToUserCommand, Result>
{
    public Task<Result> Handle(AssignPrivilegeToUserCommand request, CancellationToken cancellationToken)
        => privilegeService.AssignPrivilegeToUserAsync(
            request.UserId,
            request.PrivilegeIdOrName,
            request.Effect,
            request.ExpiresAt,
            request.Reason,
            cancellationToken);
}

public sealed class RevokePrivilegeFromUserCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<RevokePrivilegeFromUserCommand, Result>
{
    public Task<Result> Handle(RevokePrivilegeFromUserCommand request, CancellationToken cancellationToken)
        => privilegeService.RevokePrivilegeFromUserAsync(request.UserId, request.PrivilegeId, cancellationToken);
}

public sealed class AssignPolicyToRoleCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<AssignPolicyToRoleCommand, Result>
{
    public Task<Result> Handle(AssignPolicyToRoleCommand request, CancellationToken cancellationToken)
        => privilegeService.AssignPolicyToRoleAsync(request.RoleId, request.PolicyId, request.ExpiresAt, cancellationToken);
}

public sealed class AssignPolicyToUserCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<AssignPolicyToUserCommand, Result>
{
    public Task<Result> Handle(AssignPolicyToUserCommand request, CancellationToken cancellationToken)
        => privilegeService.AssignPolicyToUserAsync(request.UserId, request.PolicyId, request.ExpiresAt, cancellationToken);
}
