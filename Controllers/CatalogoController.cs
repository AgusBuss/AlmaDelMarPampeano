using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Controllers
{
    // Página pública: cualquier visitante puede mirar, nadie puede modificar nada desde acá
    [AllowAnonymous]
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? coleccionId)
        {
            var colecciones = await _context.Colecciones
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            var consulta = _context.Lamparas
                .AsNoTracking()
                .Include(l => l.Coleccion)
                .Include(l => l.MaterialCampana)
                .Include(l => l.MaterialBase)
                .Include(l => l.Fotos)
                .AsQueryable();

            if (coleccionId != null)
            {
                consulta = consulta.Where(l => l.ColeccionId == coleccionId);
            }

            var lamparas = await consulta
                .OrderBy(l => l.NumeroPieza)
                .ThenBy(l => l.Nombre)
                .ToListAsync();

            return View(new CatalogoViewModel
            {
                Colecciones = colecciones,
                ColeccionId = coleccionId,
                Lamparas = lamparas
            });
        }
    }
}