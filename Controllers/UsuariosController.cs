using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Controllers
{
    // Solo el administrador puede ver usuarios y restablecer contraseñas
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsuariosController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.Email)
                .ToListAsync();

            var lista = new List<UsuarioListaViewModel>();
            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                lista.Add(new UsuarioListaViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    Rol = roles.Count > 0 ? string.Join(", ", roles) : "Sin rol"
                });
            }

            return View(lista);
        }

        public async Task<IActionResult> Restablecer(string? id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            // No se restablecen cuentas de administradores desde acá
            if (await _userManager.IsInRoleAsync(usuario, "Administrador"))
            {
                TempData["Error"] = "Las cuentas de administrador no se restablecen desde esta pantalla. Usá \"Cambiar contraseña\".";
                return RedirectToAction(nameof(Index));
            }

            return View(new RestablecerContrasenaViewModel
            {
                Id = usuario.Id,
                Email = usuario.Email ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restablecer(RestablecerContrasenaViewModel vm)
        {
            // El email del formulario no se confía: se vuelve a leer de la base
            var usuario = await _userManager.FindByIdAsync(vm.Id);
            if (usuario == null) return NotFound();

            vm.Email = usuario.Email ?? string.Empty;

            if (await _userManager.IsInRoleAsync(usuario, "Administrador"))
            {
                TempData["Error"] = "Las cuentas de administrador no se restablecen desde esta pantalla. Usá \"Cambiar contraseña\".";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) return View(vm);

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await _userManager.ResetPasswordAsync(usuario, token, vm.ContrasenaTemporal);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(vm);
            }

            // Auditoría: quién lo hizo y a quién (nunca se guarda la contraseña)
            var admin = await _userManager.GetUserAsync(User);
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = admin?.Id ?? string.Empty,
                UsuarioEmail = admin?.Email ?? string.Empty,
                AccionRealizada = $"Restableció la contraseña de {usuario.Email}",
                TipoAccion = "Modificar",
                Entidad = "Usuario",
                EntidadId = usuario.Id,
                FechaHora = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Se restableció la contraseña de {usuario.Email}. Pasale la contraseña temporal y pedile que la cambie al ingresar.";
            return RedirectToAction(nameof(Index));
        }
    }
}