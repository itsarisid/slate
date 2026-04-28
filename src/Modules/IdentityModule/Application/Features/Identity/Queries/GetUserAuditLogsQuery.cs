using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using MediatR;

namespace Alphabet.Application.Features.Identity.Queries;

/// <summary>
/// Returns paginated audit log entries for a given user.
/// </summary>
public sealed record GetUserAuditLogsQuery(Guid UserId, int Take = 50, int Skip = 0)
    : IRequest<IReadOnlyList<AuditLogDto>>;

public sealed class GetUserAuditLogsQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUserAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    public Task<IReadOnlyList<AuditLogDto>> Handle(GetUserAuditLogsQuery request, CancellationToken cancellationToken)
        => identityService.GetUserAuditLogsAsync(request.UserId, request.Take, request.Skip, cancellationToken);
}
