// Requires: dotnet add package Bogus
//
// Seeds, in dependency order: Locations -> AssetCategories -> Assets ->
// AssetAssignments (+ denormalized Asset.AssignedToUserId) -> AssetMaintenance.
//
// USER REFERENCES: AssignedToUserId / SupplierId are just random Guids here
// since this module doesn't own the Users table. If you already have real
// user IDs (e.g. from an Identity seeder), pass them into SeedAsync via
// existingUserIds so assignments point at real users instead.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Data.Seed
{
    public class AssetSeedOptions
    {
        public int LocationCount { get; set; } = 5;
        public int CategoryCount { get; set; } = 8;
        public int AssetCount { get; set; } = 100;
        public bool WipeExisting { get; set; } = false;
        /// <summary>Real user IDs to use for assignments; if empty, random Guids are used.</summary>
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class AssetSeedResult
    {
        public int Locations { get; set; }
        public int Categories { get; set; }
        public int Assets { get; set; }
        public int Assignments { get; set; }
        public int MaintenanceRecords { get; set; }
    }

    public interface IAssetSeeder
    {
        Task<AssetSeedResult> SeedAsync(AssetSeedOptions options, CancellationToken ct = default);
    }

    public class AssetSeeder : IAssetSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        public AssetSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<AssetSeedResult> SeedAsync(AssetSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();

            // ---- Locations ----
            var locationFaker = new Faker<Location>()
                .RuleFor(l => l.Id, _ => Guid.NewGuid())
                .RuleFor(l => l.Name, f => $"{f.Address.City()} {f.PickRandom("HQ", "Campus", "Office", "Warehouse", "Branch")}")
                .RuleFor(l => l.Code, f => f.Random.AlphaNumeric(5).ToUpper())
                .RuleFor(l => l.Type, f => f.PickRandom<AssetLocationType>())
                .RuleFor(l => l.Street, f => f.Address.StreetAddress())
                .RuleFor(l => l.City, f => f.Address.City())
                .RuleFor(l => l.State, f => f.Address.State())
                .RuleFor(l => l.PostalCode, f => f.Address.ZipCode())
                .RuleFor(l => l.Country, f => f.Address.Country())
                .RuleFor(l => l.Latitude, f => f.Address.Latitude())
                .RuleFor(l => l.Longitude, f => f.Address.Longitude())
                .RuleFor(l => l.IsActive, f => f.Random.Bool(0.9f))
                .RuleFor(l => l.ContactPerson, f => f.Name.FullName())
                .RuleFor(l => l.ContactPhone, f => f.Phone.PhoneNumber());

            var locations = locationFaker.Generate(options.LocationCount);
            await _context.Set<Location>().AddRangeAsync(locations, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Asset Categories (flat, optionally with ~30% having a parent) ----
            var categoryFaker = new Faker<AssetCategory>()
                .RuleFor(c => c.Id, _ => Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Lorem.Sentence())
                .RuleFor(c => c.DepreciationRate, f => Math.Round(f.Random.Double(0.05, 0.35), 2))
                .RuleFor(c => c.DefaultLocationId, f => f.PickRandom(locations).Id);

            var categories = categoryFaker.Generate(options.CategoryCount);
            // assign ~30% a parent from the already-generated set
            foreach (var cat in categories)
            {
                if (random.NextDouble() < 0.3)
                {
                    var possibleParent = categories.Where(c => c.Id != cat.Id).OrderBy(_ => random.Next()).FirstOrDefault();
                    if (possibleParent != null)
                        cat.ParentCategoryId = possibleParent.Id;
                }
            }
            await _context.Set<AssetCategory>().AddRangeAsync(categories, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Assets ----
            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToList();

            var currencies = new[] { "USD", "EUR", "GBP", "QAR", "AED" };

            var assetFaker = new Faker<Asset>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.AssetTag, (f, a) => $"AST-{f.Random.Number(10000, 99999)}")
                .RuleFor(a => a.Name, f => f.Commerce.ProductName())
                .RuleFor(a => a.Description, f => f.Commerce.ProductDescription())
                .RuleFor(a => a.CategoryId, f => f.PickRandom(categories).Id)
                .RuleFor(a => a.Subcategory, f => f.Commerce.ProductAdjective())
                .RuleFor(a => a.Manufacturer, f => f.Company.CompanyName())
                .RuleFor(a => a.Model, f => f.Commerce.Product())
                .RuleFor(a => a.SerialNumber, f => f.Random.Replace("SN-????????"))
                .RuleFor(a => a.PurchaseDate, f => DateOnly.FromDateTime(f.Date.Past(3)))
                .RuleFor(a => a.WarrantyExpiry, (f, a) => DateOnly.FromDateTime(f.Date.Between(
                    a.PurchaseDate!.Value.ToDateTime(TimeOnly.MinValue), DateTime.UtcNow.AddYears(2))))
                .RuleFor(a => a.Cost, f => Math.Round(f.Random.Double(50, 15000), 2))
                .RuleFor(a => a.Currency, f => f.PickRandom(currencies))
                .RuleFor(a => a.Status, f => f.PickRandom<AssetStatus>())
                .RuleFor(a => a.Condition, f => f.PickRandom<AssetCondition>())
                .RuleFor(a => a.LocationId, f => f.PickRandom(locations).Id)
                .RuleFor(a => a.SupplierId, f => f.Random.Bool(0.7f) ? Guid.NewGuid() : (Guid?)null)
                .RuleFor(a => a.CreatedAt, f => f.Date.Past(2, DateTime.UtcNow))
                .RuleFor(a => a.Images, f => f.Make(f.Random.Number(0, 3), () => f.Image.PicsumUrl()))
                .RuleFor(a => a.Documents, f => new List<string>());

            var assets = assetFaker.Generate(options.AssetCount);

            // ~60% of assets are currently assigned to someone
            foreach (var asset in assets)
            {
                if (random.NextDouble() < 0.6)
                    asset.AssignedToUserId = userIds[random.Next(userIds.Count)];
            }

            await _context.Set<Asset>().AddRangeAsync(assets, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Assignment history (1-3 per assigned asset) ----
            var assignmentFaker = new Faker<AssetAssignment>()
                .RuleFor(x => x.Id, _ => Guid.NewGuid())
                .RuleFor(x => x.AssignmentType, f => f.PickRandom<AssetAssignmentType>())
                .RuleFor(x => x.Purpose, f => f.Lorem.Sentence(4))
                .RuleFor(x => x.ConditionAtAssignment, f => f.PickRandom<AssetCondition>())
                .RuleFor(x => x.Notes, f => f.Random.Bool(0.4f) ? f.Lorem.Sentence() : null);

            var assignments = new List<AssetAssignment>();
            foreach (var asset in assets.Where(a => a.AssignedToUserId != null))
            {
                var historyCount = random.Next(1, 4);
                for (int i = 0; i < historyCount; i++)
                {
                    var record = assignmentFaker.Generate();
                    record.AssetId = asset.Id;
                    record.AssignedToUserId = i == historyCount - 1
                        ? asset.AssignedToUserId!.Value
                        : userIds[random.Next(userIds.Count)];
                    record.AssignedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(30, 700)));

                    var isCurrent = i == historyCount - 1;
                    record.IsActive = isCurrent;
                    if (!isCurrent)
                    {
                        record.ActualReturnDate = record.AssignedDate.AddDays(random.Next(10, 200));
                        record.ConditionOnReturn = new Faker().PickRandom<AssetCondition>();
                    }
                    else
                    {
                        record.ExpectedReturnDate = random.NextDouble() < 0.5
                            ? record.AssignedDate.AddDays(random.Next(60, 365))
                            : null;
                    }

                    assignments.Add(record);
                }
            }
            await _context.Set<AssetAssignment>().AddRangeAsync(assignments, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Maintenance records (0-2 per asset) ----
            var maintenanceFaker = new Faker<AssetMaintenance>()
                .RuleFor(m => m.Id, _ => Guid.NewGuid())
                .RuleFor(m => m.MaintenanceType, f => f.PickRandom<AssetMaintenanceType>())
                .RuleFor(m => m.Priority, f => f.PickRandom<AssetMaintenancePriority>())
                .RuleFor(m => m.Description, f => f.Lorem.Sentence())
                .RuleFor(m => m.AssignedToVendor, f => f.Random.Bool(0.6f) ? f.Company.CompanyName() : null)
                .RuleFor(m => m.EstimatedCost, f => Math.Round(f.Random.Double(20, 2000), 2));

            var maintenanceRecords = new List<AssetMaintenance>();
            foreach (var asset in assets)
            {
                var maintenanceCount = random.Next(0, 3);
                for (int i = 0; i < maintenanceCount; i++)
                {
                    var record = maintenanceFaker.Generate();
                    record.AssetId = asset.Id;
                    record.ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(-60, 400)));
                    record.IsCompleted = record.ScheduledDate < DateOnly.FromDateTime(DateTime.UtcNow) && random.NextDouble() < 0.8;
                    if (record.IsCompleted)
                    {
                        record.CompletionDate = record.ScheduledDate.AddDays(random.Next(0, 14));
                        record.ActualCost = Math.Round(record.EstimatedCost * (0.8 + random.NextDouble() * 0.5), 2);
                        record.Notes = new Faker().Lorem.Sentence();
                        if (random.NextDouble() < 0.4)
                            record.NextMaintenanceDueDate = record.CompletionDate.Value.AddMonths(random.Next(3, 12));
                    }
                    maintenanceRecords.Add(record);
                }
            }
            await _context.Set<AssetMaintenance>().AddRangeAsync(maintenanceRecords, ct);
            await _context.SaveChangesAsync(ct);

            return new AssetSeedResult
            {
                Locations = locations.Count,
                Categories = categories.Count,
                Assets = assets.Count,
                Assignments = assignments.Count,
                MaintenanceRecords = maintenanceRecords.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            // Delete children before parents to respect FKs.
            _context.Set<AssetMaintenance>().RemoveRange(_context.Set<AssetMaintenance>());
            _context.Set<AssetAssignment>().RemoveRange(_context.Set<AssetAssignment>());
            _context.Set<Asset>().RemoveRange(_context.Set<Asset>());
            _context.Set<AssetCategory>().RemoveRange(_context.Set<AssetCategory>());
            _context.Set<Location>().RemoveRange(_context.Set<Location>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
