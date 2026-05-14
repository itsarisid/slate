using Alphabet.Application.Common.Interfaces;

namespace Alphabet.Application.Features.Productivity.Common;

internal static class ProductivityUserContext
{
    public static Guid GetRequiredUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId ?? throw new InvalidOperationException("The current user is required for productivity operations.");
    }
}
