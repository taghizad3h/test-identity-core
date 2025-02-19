using Microsoft.AspNetCore.Identity;
using TestIdentityCore.Domain;

namespace TestIdentityCore;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var roleName = "Admin";
        var userEmail = "admin@test.com";
        var userPassword = "Test@123";

        // Create the Admin role if it doesn't exist
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            var role = new ApplicationRole { Name = roleName };
            await roleManager.CreateAsync(role);
        }

        // Create a test user if it doesn't exist
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
            };

            var result = await userManager.CreateAsync(user, userPassword);
            if (result.Succeeded)
            {
                // Assign the Admin role to the user
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
