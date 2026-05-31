namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Generates JWT access tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate token async.
    /// </summary>
    Task<string> GenerateTokenAsync(Guid userId, string email, IEnumerable<string> roles, CancellationToken cancellationToken);
}
