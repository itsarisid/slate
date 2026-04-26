namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Sends SMS messages through an external provider.
/// </summary>
public interface ISmsService
{
    Task SendAsync(string to, string body, CancellationToken cancellationToken);
}
