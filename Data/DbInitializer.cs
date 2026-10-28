using Microsoft.AspNetCore.Identity;

namespace AlmaDelMarPampeano.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Crear los roles
            string[] roleNames = { "Administrador", "Cliente" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Crear el Usuario Administrador por defecto
            string adminEmail = "admin@almapampeana.com";
            string adminPass = "Admin123!"; // Contraseña con mayúscula, número y símbolo
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(newAdmin, adminPass);
                if (createPowerUser.Succeeded)
                {
                    // Le asignamos el rol
                    await userManager.AddToRoleAsync(newAdmin, "Administrador");
                }
            }
        }
    }
}