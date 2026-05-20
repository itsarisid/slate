namespace Alphabet.Domain.Interfaces.Privilege;

/// <summary>
/// Provides domain-facing privilege evaluation capabilities.
/// </summary>
public interface IPrivilegeEvaluationService
{
    /// <summary>
    /// Has privilege async.
    /// </summary>
    Task<bool> HasPrivilegeAsync(Guid userId, IEnumerable<string> privileges, bool requireAll, CancellationToken cancellationToken);
}
