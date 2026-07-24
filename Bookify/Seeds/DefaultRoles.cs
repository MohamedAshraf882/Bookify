using Microsoft.AspNetCore.Identity;

namespace Bookify.Seeds
{
    public static class DefaultRoles
    {


        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if(!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Archive));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Reception));
            }
        }


    }
}
