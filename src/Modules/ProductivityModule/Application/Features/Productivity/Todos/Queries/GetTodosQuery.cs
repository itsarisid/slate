using Alphabet.Application.Features.Productivity.Common;
using Alphabet.Application.Features.Productivity.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.Productivity.Todos.Queries;

/// <summary>
/// Gets todos using filter and paging criteria.
/// </summary>
public sealed record GetTodosQuery(
    TodoStatus? Status,
    Priority? Priority,
    string? Category,
    string? Tag,
    DateTimeOffset? DueDateFrom,
    DateTimeOffset? DueDateTo,
    Guid? AssignedTo,
    string? Search,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IRequest<Result<PagedResponseDto<TodoDto>>>;

public sealed class GetTodosQueryHandler(
    ITodoRepository todoRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetTodosQuery, Result<PagedResponseDto<TodoDto>>>
{
    public async Task<Result<PagedResponseDto<TodoDto>>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var userId = ProductivityUserContext.GetRequiredUserId(currentUserService);
        var result = await todoRepository.SearchAsync(
            new TodoQueryFilter(
                request.Status,
                request.Priority,
                request.Category,
                request.Tag,
                request.DueDateFrom,
                request.DueDateTo,
                request.AssignedTo,
                request.Search,
                request.SortBy,
                request.SortDirection,
                request.Page <= 0 ? 1 : request.Page,
                request.PageSize <= 0 ? 20 : request.PageSize,
                userId),
            cancellationToken);

        return Result<PagedResponseDto<TodoDto>>.Success(
            new PagedResponseDto<TodoDto>(result.Items.Select(x => x.ToDto()).ToArray(), result.Page, result.PageSize, result.TotalCount));
    }
}
