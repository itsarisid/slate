using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Identity;
/// <summary>
/// App db context extensions.
/// </summary>

internal static class AppDbContextExtensions
{
    /// <summary>
    /// Users of identity.
    /// </summary>
    public static DbSet<ApplicationUser> UsersOfIdentity(this AppDbContext dbContext) => dbContext.Set<ApplicationUser>();
}
