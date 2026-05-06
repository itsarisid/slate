using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Features.Privilege.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using MediatR;

namespace Alphabet.Application.Features.Privilege.Queries;

public sealed record GetPrivilegesQuery(int PageNumber = 1, int PageSize = 50, string? Category = null, string? Search = null, bool IncludeDeprecated = false)
    : IRequest<PagedResponseDto<PrivilegeDto>>;

public sealed record GetPrivilegeByIdQuery(Guid PrivilegeId) : IRequest<Result<PrivilegeDto>>;

public sealed record GetRolePrivilegesQuery(Guid RoleId) : IRequest<IReadOnlyList<PrivilegeAssignmentDto>>;

public sealed record GetUserEffectivePrivilegesQuery(Guid UserId) : IRequest<IReadOnlyList<UserEffectivePrivilegeDto>>;

public sealed record CheckUserPrivilegeQuery(Guid UserId, string PrivilegeName) : IRequest<Result<PrivilegeCheckResultDto>>;

public sealed record BatchCheckUserPrivilegesQuery(Guid UserId, IReadOnlyCollection<string> Privileges)
    : IRequest<IReadOnlyDictionary<string, bool>>;

public sealed record GetPrivilegeAnalyticsQuery() : IRequest<PrivilegeAnalyticsDto>;

public sealed record GetPrivilegeAuditLogQuery(
    Guid? UserId,
    Guid? PrivilegeId,
    PrivilegeAction? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Take = 100,
    int Skip = 0) : IRequest<IReadOnlyList<PrivilegeAuditLogDto>>;

public sealed record GetMyPrivilegesQuery() : IRequest<IReadOnlyList<UserEffectivePrivilegeDto>>;

public sealed record GetPrivilegeCategoriesQuery() : IRequest<IReadOnlyList<PrivilegeCategoryDto>>;

public sealed class GetPrivilegesQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetPrivilegesQuery, PagedResponseDto<PrivilegeDto>>
{
    public Task<PagedResponseDto<PrivilegeDto>> Handle(GetPrivilegesQuery request, CancellationToken cancellationToken)
        => privilegeService.GetPrivilegesAsync(request.PageNumber, request.PageSize, request.Category, request.Search, request.IncludeDeprecated, cancellationToken);
}

public sealed class GetPrivilegeByIdQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetPrivilegeByIdQuery, Result<PrivilegeDto>>
{
    public Task<Result<PrivilegeDto>> Handle(GetPrivilegeByIdQuery request, CancellationToken cancellationToken)
        => privilegeService.GetPrivilegeByIdAsync(request.PrivilegeId, cancellationToken);
}

public sealed class GetRolePrivilegesQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetRolePrivilegesQuery, IReadOnlyList<PrivilegeAssignmentDto>>
{
    public Task<IReadOnlyList<PrivilegeAssignmentDto>> Handle(GetRolePrivilegesQuery request, CancellationToken cancellationToken)
        => privilegeService.GetRolePrivilegesAsync(request.RoleId, cancellationToken);
}

public sealed class GetUserEffectivePrivilegesQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetUserEffectivePrivilegesQuery, IReadOnlyList<UserEffectivePrivilegeDto>>
{
    public Task<IReadOnlyList<UserEffectivePrivilegeDto>> Handle(GetUserEffectivePrivilegesQuery request, CancellationToken cancellationToken)
        => privilegeService.GetUserEffectivePrivilegesAsync(request.UserId, cancellationToken);
}

public sealed class CheckUserPrivilegeQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<CheckUserPrivilegeQuery, Result<PrivilegeCheckResultDto>>
{
    public Task<Result<PrivilegeCheckResultDto>> Handle(CheckUserPrivilegeQuery request, CancellationToken cancellationToken)
        => privilegeService.CheckPrivilegeAsync(request.UserId, request.PrivilegeName, cancellationToken);
}

public sealed class BatchCheckUserPrivilegesQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<BatchCheckUserPrivilegesQuery, IReadOnlyDictionary<string, bool>>
{
    public Task<IReadOnlyDictionary<string, bool>> Handle(BatchCheckUserPrivilegesQuery request, CancellationToken cancellationToken)
        => privilegeService.BatchCheckPrivilegesAsync(request.UserId, request.Privileges, cancellationToken);
}

public sealed class GetPrivilegeAnalyticsQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetPrivilegeAnalyticsQuery, PrivilegeAnalyticsDto>
{
    public Task<PrivilegeAnalyticsDto> Handle(GetPrivilegeAnalyticsQuery request, CancellationToken cancellationToken)
        => privilegeService.GetAnalyticsAsync(cancellationToken);
}

public sealed class GetPrivilegeAuditLogQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetPrivilegeAuditLogQuery, IReadOnlyList<PrivilegeAuditLogDto>>
{
    public Task<IReadOnlyList<PrivilegeAuditLogDto>> Handle(GetPrivilegeAuditLogQuery request, CancellationToken cancellationToken)
        => privilegeService.GetAuditLogsAsync(request.UserId, request.PrivilegeId, request.Action, request.From, request.To, request.Take, request.Skip, cancellationToken);
}

public sealed class GetMyPrivilegesQueryHandler(IPrivilegeService privilegeService, ICurrentUserService currentUserService)
    : IRequestHandler<GetMyPrivilegesQuery, IReadOnlyList<UserEffectivePrivilegeDto>>
{
    public Task<IReadOnlyList<UserEffectivePrivilegeDto>> Handle(GetMyPrivilegesQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Task.FromResult<IReadOnlyList<UserEffectivePrivilegeDto>>([]);
        }

        return privilegeService.GetUserEffectivePrivilegesAsync(currentUserService.UserId.Value, cancellationToken);
    }
}

public sealed class GetPrivilegeCategoriesQueryHandler(IPrivilegeService privilegeService)
    : IRequestHandler<GetPrivilegeCategoriesQuery, IReadOnlyList<PrivilegeCategoryDto>>
{
    public Task<IReadOnlyList<PrivilegeCategoryDto>> Handle(GetPrivilegeCategoriesQuery request, CancellationToken cancellationToken)
        => privilegeService.GetCategoriesAsync(cancellationToken);
}
