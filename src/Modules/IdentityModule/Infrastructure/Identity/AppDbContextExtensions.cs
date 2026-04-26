using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Identity;

internal static class AppDbContextExtensions
{
    public static DbSet<ApplicationUser> UsersOfIdentity(this AppDbContext dbContext) => dbContext.Set<ApplicationUser>();
}
