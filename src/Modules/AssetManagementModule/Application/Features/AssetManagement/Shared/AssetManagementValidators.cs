using Alphabet.Application.Features.AssetManagement.Assignments.Commands;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Application.Features.AssetManagement.Categories.Commands;
using Alphabet.Application.Features.AssetManagement.Inventory.Commands;
using Alphabet.Application.Features.AssetManagement.Locations.Commands;
using Alphabet.Application.Features.AssetManagement.Maintenance.Commands;
using Alphabet.Application.Features.AssetManagement.Workflows.Commands;
using FluentValidation;

namespace Alphabet.Application.Features.AssetManagement.Shared;

/// <summary>
/// Validates asset creation requests.
/// </summary>
public sealed class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
    }
}

/// <summary>
/// Validates asset updates.
/// </summary>
public sealed class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
    }
}

/// <summary>
/// Validates assignment requests.
/// </summary>
public sealed class AssignAssetCommandValidator : AbstractValidator<AssignAssetCommand>
{
    public AssignAssetCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.AssignedToUserId).NotEmpty();
        RuleFor(x => x.Purpose).MaximumLength(500);
    }
}

/// <summary>
/// Validates location creation.
/// </summary>
public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address.Street).NotEmpty();
        RuleFor(x => x.Address.City).NotEmpty();
        RuleFor(x => x.Address.Country).NotEmpty();
    }
}

/// <summary>
/// Validates category creation.
/// </summary>
public sealed class CreateAssetCategoryCommandValidator : AbstractValidator<CreateAssetCategoryCommand>
{
    public CreateAssetCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

/// <summary>
/// Validates stock adjustments.
/// </summary>
public sealed class AdjustInventoryStockCommandValidator : AbstractValidator<AdjustInventoryStockCommand>
{
    public AdjustInventoryStockCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

/// <summary>
/// Validates maintenance requests.
/// </summary>
public sealed class ScheduleAssetMaintenanceCommandValidator : AbstractValidator<ScheduleAssetMaintenanceCommand>
{
    public ScheduleAssetMaintenanceCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0);
    }
}

/// <summary>
/// Validates workflow definition creation.
/// </summary>
public sealed class CreateAssetWorkflowDefinitionCommandValidator : AbstractValidator<CreateAssetWorkflowDefinitionCommand>
{
    public CreateAssetWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Steps).NotEmpty();
    }
}
