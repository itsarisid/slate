using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Workflows.Queries;

/// <summary>
/// Gets a workflow instance.
/// </summary>
public sealed record GetAssetWorkflowInstanceQuery(Guid InstanceId) : IRequest<Result<AssetWorkflowInstanceDto>>;

/// <summary>
/// Gets pending workflow items for the current user or roles.
/// </summary>
public sealed record GetPendingAssetWorkflowsQuery() : IRequest<Result<IReadOnlyList<AssetWorkflowInstanceDto>>>;

/// <summary>
/// Handles workflow instance queries.
/// </summary>
public sealed class GetAssetWorkflowInstanceQueryHandler(IAssetRepository assetRepository)
    : IRequestHandler<GetAssetWorkflowInstanceQuery, Result<AssetWorkflowInstanceDto>>
{
    public async Task<Result<AssetWorkflowInstanceDto>> Handle(GetAssetWorkflowInstanceQuery request, CancellationToken cancellationToken)
    {
        var instance = await assetRepository.GetWorkflowInstanceByIdAsync(request.InstanceId, cancellationToken);
        return instance is null
            ? Result<AssetWorkflowInstanceDto>.Failure("Workflow instance was not found.")
            : instance.ToInstanceDto();
    }
}

/// <summary>
/// Handles pending workflow queries.
/// </summary>
public sealed class GetPendingAssetWorkflowsQueryHandler(
    IAssetRepository assetRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPendingAssetWorkflowsQuery, Result<IReadOnlyList<AssetWorkflowInstanceDto>>>
{
    public async Task<Result<IReadOnlyList<AssetWorkflowInstanceDto>>> Handle(GetPendingAssetWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var items = await assetRepository.GetPendingWorkflowInstancesAsync(currentUserService.UserId, currentUserService.Roles, cancellationToken);
        return Result<IReadOnlyList<AssetWorkflowInstanceDto>>.Success(items.Select(x => x.ToInstanceDto()).ToArray());
    }
}
