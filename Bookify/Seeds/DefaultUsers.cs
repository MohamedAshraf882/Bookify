using Microsoft.AspNetCore.Identity;

namespace Bookify.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUser(UserManager<ApplicationUser>userManger )
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                Email="admin@bookify.com",
                FullName="Admin",
                EmailConfirmed=true,
            };

            var user=await userManger.FindByEmailAsync(admin.Email);
            if (user == null) 
            {
                await userManger.CreateAsync(admin,"P@ssword123");
                await userManger.AddToRoleAsync(admin,AppRoles.Admin);
            }

        }
    }
}
