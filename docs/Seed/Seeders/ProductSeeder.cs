// Requires the Bogus NuGet package:
//   dotnet add package Bogus
//
// Adjust the namespace / DbContext type / DbSet name to match your project.
// This assumes your DbContext exposes: DbSet<Product> Products { get; set; }

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Data.Seed
{
    public interface IProductSeeder
    {
        /// <summary>
        /// Seeds the Products table with fake data.
        /// </summary>
        /// <param name="count">How many products to generate.</param>
        /// <param name="wipeExisting">If true, deletes existing products before seeding.</param>
        Task<int> SeedAsync(int count = 50, bool wipeExisting = false, CancellationToken ct = default);
    }

    public class ProductSeeder : IProductSeeder
    {
        private readonly DbContext _context; // replace DbContext with your actual AppDbContext type

        public ProductSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<int> SeedAsync(int count = 50, bool wipeExisting = false, CancellationToken ct = default)
        {
            var products = _context.Set<Product>();

            if (wipeExisting)
            {
                var existing = await products.ToListAsync(ct);
                products.RemoveRange(existing);
                await _context.SaveChangesAsync(ct);
            }

            var currencies = new[] { "USD", "EUR", "GBP", "AED", "QAR" };
            var statuses = new[] { "Active", "Active", "Active", "Draft", "Discontinued" }; // weighted toward Active

            var faker = new Faker<Product>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => Math.Round(f.Random.Double(5, 2500), 2))
                .RuleFor(p => p.Currency, f => f.PickRandom(currencies))
                .RuleFor(p => p.Status, f => f.PickRandom(statuses))
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(2, DateTime.UtcNow));

            var generated = faker.Generate(count);

            await products.AddRangeAsync(generated, ct);
            await _context.SaveChangesAsync(ct);

            return generated.Count;
        }
    }
}
