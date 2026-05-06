namespace Alphabet.Domain.Interfaces.Privilege;

/// <summary>
/// Provides domain-facing privilege evaluation capabilities.
/// </summary>
public interface IPrivilegeEvaluationService
{
    Task<bool> HasPrivilegeAsync(Guid userId, IEnumerable<string> privileges, bool requireAll, CancellationToken cancellationToken);
}
