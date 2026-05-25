using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Alphabet.Infrastructure.Repositories.AssetManagement;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Alphabet.IntegrationTests.AssetManagement;

public sealed class AssetRepositoryIntegrationTests
{
    [Fact]
    public async Task GetAssetsAsync_Should_Return_Filtered_Assets()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"asset-repo-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new AppDbContext(options);
        var repository = new AssetRepository(dbContext);

        var category = AssetCategory.Create("IT Equipment", "Devices", null, "{}", 20m, null);
        var location = Location.Create(
            "HQ",
            "HQ-01",
            AssetLocationType.Office,
            new Alphabet.Domain.ValueObjects.Address("Street", "Doha", "Doha", "00000", "Qatar"),
            null,
            true,
            null,
            null,
            null);

        dbContext.Add(category);
        dbContext.Add(location);
        await dbContext.SaveChangesAsync();

        var assetA = Asset.Create(
            "AST-000001",
            "Dell XPS 15",
            "Laptop",
            category.Id,
            "Laptop",
            "Dell",
            "XPS",
            "SN-A",
            new DateOnly(2026, 1, 1),
            new DateOnly(2028, 1, 1),
            1999.99m,
            "USD",
            AssetStatus.Available,
            AssetCondition.Good,
            location.Id,
            null,
            null,
            null,
            null,
            null,
            null);

        var assetB = Asset.Create(
            "AST-000002",
            "Warehouse Scanner",
            "Scanner",
            category.Id,
            "Scanner",
            "Zebra",
            "Z1",
            "SN-B",
            new DateOnly(2025, 1, 1),
            null,
            450m,
            "USD",
            AssetStatus.Assigned,
            AssetCondition.Good,
            location.Id,
            null,
            null,
            null,
            null,
            null,
            null);

        dbContext.Add(assetA);
        dbContext.Add(assetB);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetAssetsAsync(
            new AssetQueryFilter(
                "Available",
                category.Id,
                location.Id,
                null,
                "Dell",
                null,
                null,
                null,
                null,
                null,
                null,
                true,
                "name",
                "asc",
                1,
                20),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("AST-000001", result.Items[0].AssetTag);
    }
}
