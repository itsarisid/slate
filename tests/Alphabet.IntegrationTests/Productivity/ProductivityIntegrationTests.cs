using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Persistence.Context;
using Alphabet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Alphabet.IntegrationTests.Productivity;

public sealed class ProductivityIntegrationTests
{
    [Fact]
    public async Task TodoRepository_Should_Filter_By_Search_Text()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var dbContext = new AppDbContext(options);
        var todo = Todo.Create(Guid.NewGuid(), null, "Finish proposal", "Write Q2 proposal", Alphabet.Domain.Enums.Priority.High, DateTimeOffset.UtcNow.AddDays(1), "Work", null, false, null);
        dbContext.ProductivityTodos.Add(todo);
        await dbContext.SaveChangesAsync();

        var repository = new TodoRepository(dbContext);
        var result = await repository.SearchAsync(
            new Alphabet.Domain.Models.TodoQueryFilter(null, null, null, null, null, null, null, "proposal", null, null, 1, 10, null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Finish proposal", result.Items[0].Title);
    }
}
