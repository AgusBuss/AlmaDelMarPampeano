using AlmaDelMarPampeano.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Data
{
    public static class DbInitializer
    {
        public const string RolAdministrador = "Administrador";
        public const string RolCliente = "Cliente";

        // Credenciales de PRUEBA para la corrección (van también en el README)
        public const string AdminEmail = "admin@almapampeana.com";
        public const string AdminPass = "Admin123!";
        public const string ClienteEmail = "cliente@almapampeana.com";
        public const string ClientePass = "Cliente123!";

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Roles
            foreach (var roleName in new[] { RolAdministrador, RolCliente })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Usuarios de prueba
            await CrearUsuarioSiNoExiste(userManager, AdminEmail, AdminPass, RolAdministrador);
            await CrearUsuarioSiNoExiste(userManager, ClienteEmail, ClientePass, RolCliente);

            // 3. Estados de pedido (el primero, "Pendiente", es el estado inicial de todo pedido nuevo)
            if (!await context.EstadosPedido.AnyAsync())
            {
                context.EstadosPedido.AddRange(
                    new EstadoPedido { NombreEstado = "Pendiente" },
                    new EstadoPedido { NombreEstado = "En producción" },
                    new EstadoPedido { NombreEstado = "Terminado" },
                    new EstadoPedido { NombreEstado = "Entregado" },
                    new EstadoPedido { NombreEstado = "Cancelado" });
                await context.SaveChangesAsync();
            }

            // 4. Tipos de luz que ofrece la marca
            if (!await context.TiposLuz.AnyAsync())
            {
                context.TiposLuz.AddRange(
                    new TipoLuz { Nombre = "Luz fría", Descripcion = "Luz blanca, clara y neutra." },
                    new TipoLuz { Nombre = "Luz cálida", Descripcion = "Luz amarillenta, suave y acogedora." },
                    new TipoLuz { Nombre = "Multicolor", Descripcion = "Luz LED que cambia de color." });
                await context.SaveChangesAsync();
            }
        }

        private static async Task CrearUsuarioSiNoExiste(
            UserManager<IdentityUser> userManager, string email, string password, string rol)
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario == null)
            {
                usuario = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

                // CreateAsync(user) + AddPasswordAsync en lugar de CreateAsync(user, password)
                // para que AppUserManager no le agregue el rol "Cliente" al administrador.
                var creado = await userManager.CreateAsync(usuario);
                if (!creado.Succeeded) return;

                var conClave = await userManager.AddPasswordAsync(usuario, password);
                if (!conClave.Succeeded) return;
            }

            if (!await userManager.IsInRoleAsync(usuario, rol))
            {
                await userManager.AddToRoleAsync(usuario, rol);
            }
        }
    }
}