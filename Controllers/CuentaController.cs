using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AlmaDelMarPampeano.Controllers
{
    // Solo para usuarios con sesión iniciada (clientes y administradores)
    [Authorize]
    public class CuentaController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public CuentaController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult CambiarContrasena()
        {
            return View(new CambiarContrasenaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Siempre se cambia la contraseña del usuario que tiene la sesión iniciada,
            // nunca la de un id que llegue desde el formulario.
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            var resultado = await _userManager.ChangePasswordAsync(usuario, vm.ContrasenaActual, vm.ContrasenaNueva);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(vm);
            }

            // Se renueva la sesión para que no se cierre al cambiar la clave
            await _signInManager.RefreshSignInAsync(usuario);

            // Queda registrado en la auditoría (nunca la contraseña, solo el hecho)
            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuario.Id,
                UsuarioEmail = usuario.Email,
                AccionRealizada = "Cambió su contraseña",
                TipoAccion = "Modificar",
                Entidad = "Usuario",
                EntidadId = usuario.Id,
                FechaHora = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Tu contraseña se cambió correctamente.";
            return RedirectToAction("Index", "Catalogo");
        }
    }
}