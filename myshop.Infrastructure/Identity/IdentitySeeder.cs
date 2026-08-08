using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using myshop.Domain.Constants;

namespace myshop.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            foreach (var role in new[] { Roles.Admin, Roles.Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = configuration["SeedAdmin:Email"];
            var adminPassword = configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                return;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Administrator",
                    Address = string.Empty,
                    City = string.Empty,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                    return;
            }

            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}
