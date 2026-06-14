using Alphabet.Application.Features.AssetManagement.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.ValueObjects;
using MediatR;

namespace Alphabet.Application.Features.AssetManagement.Locations.Commands;

/// <summary>
/// Creates a location for asset storage or assignment.
/// </summary>
public sealed record CreateLocationCommand(
    string Name,
    string Code,
    AssetLocationType Type,
    Address Address,
    Guid? ParentLocationId,
    bool IsActive,
    Coordinates? Coordinates,
    string? ContactPerson,
    string? ContactPhone) : IRequest<Result<AssetMutationResultDto>>;

/// <summary>
/// Handles location creation.
/// </summary>
public sealed class CreateLocationCommandHandler(
    IAssetRepository assetRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLocationCommand, Result<AssetMutationResultDto>>
{
    public async Task<Result<AssetMutationResultDto>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = Location.Create(
            request.Name,
            request.Code,
            request.Type,
            request.Address,
            request.ParentLocationId,
            request.IsActive,
            request.Coordinates,
            request.ContactPerson,
            request.ContactPhone);

        await assetRepository.AddLocationAsync(location, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AssetMutationResultDto>.Success(new AssetMutationResultDto(location.Id, $"Location '{location.Name}' created successfully."));
    }
}
