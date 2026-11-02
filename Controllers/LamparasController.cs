using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LamparasController : Controller
    {
        private const long TamanoMaximoImagen = 5 * 1024 * 1024; // 5 MB por foto
        private const int MaximoFotos = 8;
        private const int LimiteSolicitud = 60 * 1024 * 1024;    // 60 MB por envío del formulario
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png" };

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LamparasController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ---------------------------------------------------------------- LISTADO
        public async Task<IActionResult> Index()
        {
            var lamparas = await _context.Lamparas
                .Include(l => l.Coleccion)
                .Include(l => l.MaterialCampana)
                .Include(l => l.MaterialBase)
                .Include(l => l.Fotos)
                .OrderBy(l => l.NumeroPieza)
                .ThenBy(l => l.Nombre)
                .ToListAsync();
            return View(lamparas);
        }

        // ---------------------------------------------------------------- CREAR
        public async Task<IActionResult> Create()
        {
            var vm = new LamparaFormViewModel();
            await CargarListasAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(LimiteSolicitud)]
        [RequestFormLimits(MultipartBodyLengthLimit = LimiteSolicitud)]
        public async Task<IActionResult> Create(LamparaFormViewModel vm)
        {
            await ValidarFormularioAsync(vm, fotosQueQuedan: 0);

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(vm);
                return View(vm);
            }

            var lampara = new Lampara
            {
                Nombre = vm.Nombre.Trim(),
                Descripcion = vm.Descripcion,
                NumeroPieza = vm.NumeroPieza,
                Precio = vm.Precio!.Value,
                Disponible = vm.Disponible,
                ColeccionId = vm.ColeccionId,
                MaterialCampanaId = vm.MaterialCampanaId,
                MaterialBaseId = vm.MaterialBaseId
            };

            await AgregarFotosAsync(lampara, vm.Imagenes);

            _context.Lamparas.Add(lampara);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "La lámpara se agregó al catálogo.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lampara = await _context.Lamparas
                .Include(l => l.Fotos)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lampara == null) return NotFound();

            var vm = new LamparaFormViewModel
            {
                Id = lampara.Id,
                Nombre = lampara.Nombre,
                Descripcion = lampara.Descripcion,
                NumeroPieza = lampara.NumeroPieza,
                Precio = lampara.Precio,
                Disponible = lampara.Disponible,
                ColeccionId = lampara.ColeccionId,
                MaterialCampanaId = lampara.MaterialCampanaId,
                MaterialBaseId = lampara.MaterialBaseId,
                FotosActuales = lampara.Fotos.OrderBy(f => f.Orden).ToList()
            };
            await CargarListasAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(LimiteSolicitud)]
        [RequestFormLimits(MultipartBodyLengthLimit = LimiteSolicitud)]
        public async Task<IActionResult> Edit(LamparaFormViewModel vm)
        {
            var lampara = await _context.Lamparas
                .Include(l => l.Fotos)
                .FirstOrDefaultAsync(l => l.Id == vm.Id);
            if (lampara == null) return NotFound();

            var idsABorrar = vm.FotosAEliminar ?? new List<int>();
            var fotosQueQuedan = lampara.Fotos.Count(f => !idsABorrar.Contains(f.Id));

            await ValidarFormularioAsync(vm, fotosQueQuedan);

            if (!ModelState.IsValid)
            {
                vm.FotosActuales = lampara.Fotos.OrderBy(f => f.Orden).ToList();
                await CargarListasAsync(vm);
                return View(vm);
            }

            lampara.Nombre = vm.Nombre.Trim();
            lampara.Descripcion = vm.Descripcion;
            lampara.NumeroPieza = vm.NumeroPieza;
            lampara.Precio = vm.Precio!.Value;
            lampara.Disponible = vm.Disponible;
            lampara.ColeccionId = vm.ColeccionId;
            lampara.MaterialCampanaId = vm.MaterialCampanaId;
            lampara.MaterialBaseId = vm.MaterialBaseId;

            // Solo se pueden quitar fotos que pertenecen a ESTA lámpara
            var fotosABorrar = lampara.Fotos.Where(f => idsABorrar.Contains(f.Id)).ToList();
            var rutasABorrar = fotosABorrar.Select(f => f.RutaArchivo).ToList();
            _context.FotosLampara.RemoveRange(fotosABorrar);

            await AgregarFotosAsync(lampara, vm.Imagenes);

            await _context.SaveChangesAsync();

            // Los archivos viejos se borran del disco solo si la base se guardó bien
            foreach (var ruta in rutasABorrar) BorrarImagen(ruta);

            TempData["Exito"] = "Los cambios de la lámpara se guardaron correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- BORRAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lampara = await _context.Lamparas
                .Include(l => l.Fotos)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lampara == null) return NotFound();

            var rutas = lampara.Fotos.Select(f => f.RutaArchivo).ToList();

            _context.Lamparas.Remove(lampara); // sus fotos se borran en cascada
            await _context.SaveChangesAsync();

            foreach (var ruta in rutas) BorrarImagen(ruta);

            TempData["Exito"] = "La lámpara se eliminó del catálogo.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- AUXILIARES
        private static List<IFormFile> ArchivosConContenido(IEnumerable<IFormFile>? archivos) =>
            archivos?.Where(a => a.Length > 0).ToList() ?? new List<IFormFile>();

        private async Task ValidarFormularioAsync(LamparaFormViewModel vm, int fotosQueQuedan)
        {
            if (vm.ColeccionId != null &&
                !await _context.Colecciones.AnyAsync(c => c.Id == vm.ColeccionId))
            {
                ModelState.AddModelError(nameof(vm.ColeccionId), "La colección elegida no es válida.");
            }

            if (vm.MaterialCampanaId != null &&
                !await _context.MaterialesCampana.AnyAsync(c => c.Id == vm.MaterialCampanaId))
            {
                ModelState.AddModelError(nameof(vm.MaterialCampanaId), "La campana elegida no es válida.");
            }

            if (vm.MaterialBaseId != null &&
                !await _context.MaterialesBase.AnyAsync(b => b.Id == vm.MaterialBaseId))
            {
                ModelState.AddModelError(nameof(vm.MaterialBaseId), "La base elegida no es válida.");
            }

            var nuevas = ArchivosConContenido(vm.Imagenes);

            if (fotosQueQuedan + nuevas.Count > MaximoFotos)
            {
                ModelState.AddModelError(nameof(vm.Imagenes), $"Una lámpara puede tener hasta {MaximoFotos} fotos.");
            }

            foreach (var archivo in nuevas)
            {
                if (archivo.Length > TamanoMaximoImagen)
                {
                    ModelState.AddModelError(nameof(vm.Imagenes), $"La foto \"{archivo.FileName}\" supera el tamaño máximo permitido (5 MB).");
                }

                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError(nameof(vm.Imagenes), $"La foto \"{archivo.FileName}\" tiene un formato no permitido. Solo se aceptan JPG o PNG.");
                }
            }
        }

        // Guarda cada archivo en disco y lo agrega a la lámpara con su posición en la pasarela
        private async Task AgregarFotosAsync(Lampara lampara, IEnumerable<IFormFile>? archivos)
        {
            var siguienteOrden = lampara.Fotos.Any() ? lampara.Fotos.Max(f => f.Orden) + 1 : 0;

            foreach (var archivo in ArchivosConContenido(archivos))
            {
                lampara.Fotos.Add(new FotoLampara
                {
                    RutaArchivo = await GuardarImagenAsync(archivo),
                    Orden = siguienteOrden++
                });
            }
        }

        private async Task<string> GuardarImagenAsync(IFormFile imagen)
        {
            var carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "lamparas");
            Directory.CreateDirectory(carpeta);

            // El nombre en disco lo genera el servidor (nunca el que manda el usuario)
            var nombre = Guid.NewGuid().ToString("N") + Path.GetExtension(imagen.FileName).ToLowerInvariant();
            using (var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }
            return "/lamparas/" + nombre;
        }

        private void BorrarImagen(string? rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa)) return;

            var ruta = Path.Combine(_webHostEnvironment.WebRootPath, "lamparas", Path.GetFileName(rutaRelativa));
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }

        private async Task CargarListasAsync(LamparaFormViewModel vm)
        {
            var colecciones = await _context.Colecciones.OrderBy(c => c.Id).ToListAsync();
            var campanas = await _context.MaterialesCampana.OrderBy(c => c.Nombre).ToListAsync();
            var bases = await _context.MaterialesBase.OrderBy(b => b.TipoMaterial).ToListAsync();

            vm.Colecciones = colecciones.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));
            vm.Campanas = campanas.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));
            vm.Bases = bases.Select(b => new SelectListItem(b.TipoMaterial, b.Id.ToString()));
        }
    }
}