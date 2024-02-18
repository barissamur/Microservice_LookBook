using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Api.SeedData;

public class SeedCreateRoles
{
    public static async Task CreateRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Admin", "User", "Manager" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Rol yoksa oluştur
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

}
