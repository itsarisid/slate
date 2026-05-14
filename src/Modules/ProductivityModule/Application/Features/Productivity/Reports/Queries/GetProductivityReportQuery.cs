using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Application.Features.Productivity.Common;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Reports.Queries;

/// <summary>
/// Gets a productivity report for the current user.
/// </summary>
public sealed record GetProductivityReportQuery(string Period, DateTimeOffset? Start, DateTimeOffset? End) : IRequest<Result<ProductivityReportDto>>;

public sealed class GetProductivityReportQueryHandler(
    IProductivityReadService readService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProductivityReportQuery, Result<ProductivityReportDto>>
{
    public async Task<Result<ProductivityReportDto>> Handle(GetProductivityReportQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var report = await readService.GetReportAsync(userId, request.Period, request.Start, request.End, cancellationToken);
        return Result<ProductivityReportDto>.Success(report);
    }
}
