using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Audit.Commands;

/// <summary>
/// Generates an audit report request.
/// </summary>
public sealed record GenerateAssetAuditReportCommand(
    DateTimeOffset Start,
    DateTimeOffset End,
    string Format,
    bool IncludeDeleted) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Handles audit report generation requests.
/// </summary>
public sealed class GenerateAssetAuditReportCommandHandler(
    IAssetRepository assetRepository,
    IBackgroundJobService backgroundJobService)
    : IRequestHandler<GenerateAssetAuditReportCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(GenerateAssetAuditReportCommand request, CancellationToken cancellationToken)
    {
        var reportId = Guid.NewGuid();
        await backgroundJobService.EnqueueAsync(
            $"asset-audit-report:{reportId}",
            async ct =>
            {
                _ = await assetRepository.GetActivityAsync(
                    new AssetActivityFilter(null, null, request.Start, request.End, null, 5000, 0),
                    ct);
            },
            cancellationToken);

        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(reportId, $"Audit report generation queued in {request.Format.ToUpperInvariant()} format."));
    }
}
