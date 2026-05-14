using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Productivity.CrossEntity.Queries;

/// <summary>
/// Gets today's productivity dashboard.
/// </summary>
public sealed record GetDashboardTodayQuery : IRequest<Result<DashboardDto>>;

public sealed class GetDashboardTodayQueryHandler(
    IProductivityReadService readService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetDashboardTodayQuery, Result<DashboardDto>>
{
    public async Task<Result<DashboardDto>> Handle(GetDashboardTodayQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var dashboard = await readService.GetDashboardAsync(userId, DateTimeOffset.UtcNow, cancellationToken);
        return Result<DashboardDto>.Success(dashboard);
    }
}
