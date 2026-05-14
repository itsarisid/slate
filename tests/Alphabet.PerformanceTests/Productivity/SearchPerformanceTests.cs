using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Persistence.Context;
using Alphabet.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Alphabet.PerformanceTests.Productivity;

public sealed class SearchPerformanceTests
{
    [Fact]
    public async Task Note_Search_Should_Complete_Quickly_For_Moderate_Dataset()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var dbContext = new AppDbContext(options);
        var ownerId = Guid.NewGuid();
        for (var i = 0; i < 250; i++)
        {
            dbContext.ProductivityNotes.Add(Note.Create(ownerId, $"Project idea {i}", $"Meeting notes and project idea {i}", Alphabet.Domain.Enums.NoteFormat.Markdown, "Work", "#ffffff", false, false, null, null));
        }

        await dbContext.SaveChangesAsync();
        var service = new NoteSearchService(dbContext);
        var started = DateTimeOffset.UtcNow;
        var results = await service.SearchAsync(ownerId, "project idea", CancellationToken.None);
        var elapsed = DateTimeOffset.UtcNow - started;

        Assert.NotEmpty(results);
        Assert.True(elapsed < TimeSpan.FromSeconds(5), $"Search took too long: {elapsed}.");
    }
}
