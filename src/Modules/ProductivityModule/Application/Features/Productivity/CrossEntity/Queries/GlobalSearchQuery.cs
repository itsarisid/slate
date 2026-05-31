using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Productivity.CrossEntity.Queries;

/// <summary>
/// Executes global productivity search.
/// </summary>
public sealed record GlobalSearchQuery(string Query, IReadOnlyCollection<string>? Types) : IRequest<Result<IReadOnlyList<SearchResultDto>>>;
/// <summary>
/// Global search query handler.
/// </summary>

public sealed class GlobalSearchQueryHandler(
    IProductivityReadService readService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GlobalSearchQuery, Result<IReadOnlyList<SearchResultDto>>>
{
    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<Result<IReadOnlyList<SearchResultDto>>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var items = await readService.GlobalSearchAsync(userId, request.Query, request.Types, cancellationToken);
        return Result<IReadOnlyList<SearchResultDto>>.Success(items);
    }
}
