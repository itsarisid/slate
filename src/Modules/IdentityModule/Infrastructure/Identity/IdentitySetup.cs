using Alphabet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Infrastructure.Identity;

/// <summary>
/// Seeds roles and a default administrator account.
/// </summary>                                                                                      
public static class IdentitySetup
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "User", "CatalogManager" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        const string adminEmail = "admin@alphabet.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            await userManager.CreateAsync(admin, "Admin12345!");
            await userManager.AddToRolesAsync(admin, ["Admin", "User", "CatalogManager"]);
        }
    }
}
