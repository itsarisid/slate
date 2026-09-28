namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>Creates repeat-safe sample productivity data for a user.</summary>
public interface IDemoDataSeeder
{
    Task<DemoDataSeedResult> SeedAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed record DemoDataSeedResult(int Todos, int Tasks, int Notes, int Events);
