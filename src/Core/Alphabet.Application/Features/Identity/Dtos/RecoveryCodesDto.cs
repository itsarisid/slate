namespace Alphabet.Application.Features.Identity.Dtos;

/// <summary>
/// Represents recovery codes issued for MFA backup.
/// </summary>
public sealed record RecoveryCodesDto(IReadOnlyCollection<string> Codes);
