using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Privilege.Commands;

public sealed record CreatePrivilegeCategoryCommand(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int SortOrder) : IRequest<Result<Guid>>;

public sealed record MovePrivilegeCategoryCommand(Guid PrivilegeId, Guid CategoryId) : IRequest<Result>;

public sealed record CreatePrivilegePolicyCommand(
    string Name,
    string? Description,
    IReadOnlyCollection<string> Privileges,
    PrivilegePolicyCondition Condition) : IRequest<Result<Guid>>;

public sealed record CreatePrivilegeRequestCommand(
    string PrivilegeIdOrName,
    string Reason,
    int RequestedDurationDays,
    string? ApproverEmail) : IRequest<Result<Guid>>;

public sealed record ApprovePrivilegeRequestCommand(Guid RequestId, string? Notes) : IRequest<Result>;

public sealed record DenyPrivilegeRequestCommand(Guid RequestId, string? Notes) : IRequest<Result>;

public sealed class CreatePrivilegeCategoryCommandValidator : AbstractValidator<CreatePrivilegeCategoryCommand>
{
    public CreatePrivilegeCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreatePrivilegePolicyCommandValidator : AbstractValidator<CreatePrivilegePolicyCommand>
{
    public CreatePrivilegePolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Privileges).NotEmpty();
    }
}

public sealed class CreatePrivilegeRequestCommandValidator : AbstractValidator<CreatePrivilegeRequestCommand>
{
    public CreatePrivilegeRequestCommandValidator()
    {
        RuleFor(x => x.PrivilegeIdOrName).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.RequestedDurationDays).InclusiveBetween(1, 30);
    }
}

public sealed class CreatePrivilegeCategoryCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<CreatePrivilegeCategoryCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePrivilegeCategoryCommand request, CancellationToken cancellationToken)
        => privilegeService.CreateCategoryAsync(request.Name, request.Description, request.ParentCategoryId, request.SortOrder, cancellationToken);
}

public sealed class MovePrivilegeCategoryCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<MovePrivilegeCategoryCommand, Result>
{
    public Task<Result> Handle(MovePrivilegeCategoryCommand request, CancellationToken cancellationToken)
        => privilegeService.MovePrivilegeCategoryAsync(request.PrivilegeId, request.CategoryId, cancellationToken);
}

public sealed class CreatePrivilegePolicyCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<CreatePrivilegePolicyCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePrivilegePolicyCommand request, CancellationToken cancellationToken)
        => privilegeService.CreatePolicyAsync(request.Name, request.Description, request.Privileges, request.Condition, cancellationToken);
}

public sealed class CreatePrivilegeRequestCommandHandler(
    IPrivilegeService privilegeService,
    Alphabet.Application.Common.Interfaces.ICurrentUserService currentUserService)
    : IRequestHandler<CreatePrivilegeRequestCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePrivilegeRequestCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Task.FromResult(Result<Guid>.Failure("An authenticated user is required."));
        }

        return privilegeService.CreatePrivilegeRequestAsync(
            currentUserService.UserId.Value,
            request.PrivilegeIdOrName,
            request.Reason,
            request.RequestedDurationDays,
            request.ApproverEmail,
            cancellationToken);
    }
}

public sealed class ApprovePrivilegeRequestCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<ApprovePrivilegeRequestCommand, Result>
{
    public Task<Result> Handle(ApprovePrivilegeRequestCommand request, CancellationToken cancellationToken)
        => privilegeService.ApprovePrivilegeRequestAsync(request.RequestId, request.Notes, cancellationToken);
}

public sealed class DenyPrivilegeRequestCommandHandler(IPrivilegeService privilegeService)
    : IRequestHandler<DenyPrivilegeRequestCommand, Result>
{
    public Task<Result> Handle(DenyPrivilegeRequestCommand request, CancellationToken cancellationToken)
        => privilegeService.DenyPrivilegeRequestAsync(request.RequestId, request.Notes, cancellationToken);
}
