namespace Alphabet.Application.Features.Productivity.Common;
/// <summary>
/// Productivity user context.
/// </summary>

internal static class ProductivityUserContext
{
    /// <summary>
    /// Get required user id.
    /// </summary>
    public static Guid GetRequiredUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId ?? throw new InvalidOperationException("The current user is required for productivity operations.");
    }
}
