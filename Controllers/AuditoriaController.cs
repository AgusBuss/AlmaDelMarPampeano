using AlmaDelMarPampeano.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AuditoriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? usuario)
        {
            var query = _context.Auditorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(usuario))
            {
                query = query.Where(a => a.UsuarioEmail != null && a.UsuarioEmail.Contains(usuario));
            }

            ViewData["Usuario"] = usuario;

            var registros = await query
                .OrderByDescending(a => a.FechaHora)
                .Take(200)
                .ToListAsync();

            return View(registros);
        }
    }
}