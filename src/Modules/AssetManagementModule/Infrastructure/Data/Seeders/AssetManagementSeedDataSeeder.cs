using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using Alphabet.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds default categories, locations, and workflow definitions for asset management.
/// </summary>
public static class AssetManagementSeedDataSeeder
{
    /// <summary>
    /// Seeds default module data.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Alphabet.AssetManagementSeed");

        var locations = await repository.GetLocationsAsync(CancellationToken.None);
        if (locations.Count == 0)
        {
            var headOffice = Location.Create(
                "Head Office - IT Depot",
                "HO-IT",
                Alphabet.Domain.Enums.AssetLocationType.Office,
                new Address("1 Innovation Drive", "Doha", "Doha", "00000", "Qatar"),
                null,
                true,
                new Coordinates(25.2854m, 51.5310m),
                "IT Operations",
                "+97400000000");

            var warehouse = Location.Create(
                "Main Warehouse",
                "WH-01",
                Alphabet.Domain.Enums.AssetLocationType.Warehouse,
                new Address("12 Supply Chain Street", "Doha", "Doha", "00001", "Qatar"),
                null,
                true,
                null,
                "Warehouse Manager",
                "+97400000001");

            await repository.AddLocationAsync(headOffice, CancellationToken.None);
            await repository.AddLocationAsync(warehouse, CancellationToken.None);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
            locations = [headOffice, warehouse];
        }

        var categories = await repository.GetCategoriesAsync(CancellationToken.None);
        if (categories.Count == 0)
        {
            var defaultLocationId = locations.First().Id;
            await repository.AddCategoryAsync(
                AssetCategory.Create(
                    "IT Equipment",
                    "Computers, peripherals, and related devices.",
                    null,
                    AssetManagementJson.Serialize(new Dictionary<string, string>
                    {
                        ["processor"] = "string",
                        ["ram"] = "string"
                    }),
                    20m,
                    defaultLocationId),
                CancellationToken.None);

            await repository.AddCategoryAsync(
                AssetCategory.Create(
                    "Facilities Equipment",
                    "Office and facility managed equipment.",
                    null,
                    AssetManagementJson.Serialize(new Dictionary<string, string>()),
                    10m,
                    defaultLocationId),
                CancellationToken.None);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        var workflows = await repository.GetWorkflowDefinitionsAsync(CancellationToken.None);
        if (workflows.Count == 0)
        {
            var definition = AssetWorkflowDefinition.Create(
                "Asset Procurement Workflow",
                "Default procurement and approval workflow for new asset requests.",
                1,
                [
                    new AssetWorkflowStepDefinitionModel(Guid.NewGuid(), "Request Submission", 1, "Requester", 0, 24, ["Approve", "Reject", "RequestChanges"]),
                    new AssetWorkflowStepDefinitionModel(Guid.NewGuid(), "Manager Approval", 2, "Admin", 1, 48, ["Approve", "Reject"]),
                    new AssetWorkflowStepDefinitionModel(Guid.NewGuid(), "IT Setup", 3, "Admin", 0, 72, ["Complete", "Delegate"])
                ]);

            await repository.AddWorkflowDefinitionAsync(definition, CancellationToken.None);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        logger.LogInformation("Asset management seed data ensured.");
    }
}
