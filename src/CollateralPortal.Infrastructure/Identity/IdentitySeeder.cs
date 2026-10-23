using CollateralPortal.Application.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CollateralPortal.Infrastructure.Identity;

/// <summary>
/// Seeds the workflow roles (and one bootstrap Admin user) on startup so
/// the app is usable immediately in dev — remove/gate the admin user
/// creation before going to production.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string adminEmail = "admin@collateralportal.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Portal Administrator",
                EmailConfirmed = true
            };

            // Dev-only bootstrap password — change immediately, or remove this
            // block once real accounts are provisioned via /api/auth/register.
            var result = await userManager.CreateAsync(admin, "ChangeMe123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
