using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
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
        private const int LamparasEnPortada = 3;

        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Inicio: portada, colecciones, un adelanto de lámparas y cuadernos
        public async Task<IActionResult> Index()
        {
            var colecciones = await _context.Colecciones
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            // El adelanto muestra primero las piezas disponibles
            var adelanto = await ConsultaLamparas()
                .OrderByDescending(l => l.Disponible)
                .ThenBy(l => l.NumeroPieza)
                .ThenBy(l => l.Nombre)
                .Take(LamparasEnPortada)
                .ToListAsync();

            return View(new CatalogoViewModel
            {
                Colecciones = colecciones,
                Lamparas = adelanto
            });
        }

        // Página dedicada: todas las lámparas, con filtro por colección
        public async Task<IActionResult> Lamparas(int? coleccionId)
        {
            var colecciones = await _context.Colecciones
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            var consulta = ConsultaLamparas();

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

        // Consulta base compartida por las dos páginas
        private IQueryable<Lampara> ConsultaLamparas() =>
            _context.Lamparas
                .AsNoTracking()
                .Include(l => l.Coleccion)
                .Include(l => l.MaterialCampana)
                .Include(l => l.MaterialBase)
                .Include(l => l.Fotos);
    }
}