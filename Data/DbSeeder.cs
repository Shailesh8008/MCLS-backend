using MCLS.Models;
using Microsoft.AspNetCore.Identity;

namespace MCLS.Data
{
    public class DbSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider, string adminEmail, string adminPassword)
        {
            try
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roles = { "Admin", "Captain" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var adminExists = await userManager.FindByEmailAsync(adminEmail);
                if (adminExists == null)
                {
                    var admin = new User
                    {
                        UserName = adminEmail,
                        Name = adminEmail,
                        Email = adminEmail,
                        Rank = "Fleet Manager",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(admin, adminPassword);
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error Seeding initial data (Roles and Admin details)");
                throw;
            }
        }
    }
}
