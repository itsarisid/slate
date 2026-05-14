using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Permanently removes expired trash items.
/// </summary>
public sealed class TrashCleanupJob(
    AppDbContext dbContext,
    IOptions<ProductivitySettings> settings,
    ILogger<TrashCleanupJob> logger)
{
    public async Task ExecuteAsync()
    {
        var threshold = DateTimeOffset.UtcNow.AddDays(-settings.Value.TrashRetentionDays);
        var todos = await dbContext.Set<Todo>().Where(x => x.IsDeleted && x.UpdatedAt <= threshold).ToArrayAsync();
        dbContext.RemoveRange(todos);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Trash cleanup removed {Count} todos.", todos.Length);
    }
}
