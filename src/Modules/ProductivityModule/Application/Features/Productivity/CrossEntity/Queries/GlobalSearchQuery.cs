using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Productivity.CrossEntity.Queries;

/// <summary>
/// Executes global productivity search.
/// </summary>
public sealed record GlobalSearchQuery(string Query, IReadOnlyCollection<string>? Types) : IRequest<Result<IReadOnlyList<SearchResultDto>>>;

public sealed class GlobalSearchQueryHandler(
    IProductivityReadService readService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GlobalSearchQuery, Result<IReadOnlyList<SearchResultDto>>>
{
    public async Task<Result<IReadOnlyList<SearchResultDto>>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var items = await readService.GlobalSearchAsync(userId, request.Query, request.Types, cancellationToken);
        return Result<IReadOnlyList<SearchResultDto>>.Success(items);
    }
}
