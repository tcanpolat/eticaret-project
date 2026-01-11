using Microsoft.AspNetCore.Identity;

namespace ETICARET.WebUI.Identity
{
    public static class SeedIdentity
    {
        public static async Task Seed(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager,IConfiguration configuration)
        {
            var username = configuration["Data:AdminUser:username"];
            var email = configuration["Data:AdminUser:email"];
            var password = configuration["Data:AdminUser:password"];
            var role = configuration["Data:AdminUser:role"];

            if(await userManager.FindByEmailAsync(email) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(role));

                ApplicationUser user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FullName = "Tahsin Canpolat",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user,password);

                if (result.Succeeded)
                {
                   userManager.AddToRoleAsync(user, role).Wait();
                }
            }
        }
    }
}
