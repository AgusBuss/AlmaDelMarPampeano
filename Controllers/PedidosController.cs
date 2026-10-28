using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private const string EstadoInicial = "Pendiente";
        private const long TamanoMaximoArchivo = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".pdf" };
        private static readonly string[] TiposDocumento = { "Referencia", "Comprobante", "Presupuesto" };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PedidosController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool EsAdmin => User.IsInRole("Administrador");

        /// <summary>
        /// ÚNICO punto de acceso a un pedido por id: el Administrador ve cualquiera,
        /// un Cliente solo los suyos. Si no corresponde devuelve null (y la acción responde 404).
        /// </summary>
        private async Task<Pedido?> ObtenerPedidoPermitidoAsync(int id, bool incluirRelaciones = false)
        {
            var query = _context.Pedidos.Include(p => p.EstadoPedido).AsQueryable();

            if (incluirRelaciones)
            {
                query = query
                    .Include(p => p.Detalles).ThenInclude(d => d.MaterialCampana)
                    .Include(p => p.Detalles).ThenInclude(d => d.MaterialBase)
                    .Include(p => p.Archivos);
            }

            if (!EsAdmin)
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(p => p.UsuarioId == userId);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        private bool PuedeBorrar(Pedido pedido) =>
            EsAdmin || pedido.EstadoPedido?.NombreEstado == EstadoInicial;

        // ---------------------------------------------------------------- LISTADO
        public async Task<IActionResult> Index()
        {
            var query = _context.Pedidos.Include(p => p.EstadoPedido).AsQueryable();

            if (!EsAdmin)
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(p => p.UsuarioId == userId);
            }

            return View(await query.OrderByDescending(p => p.FechaSolicitud).ToListAsync());
        }

        // ---------------------------------------------------------------- CREAR
        public IActionResult Create()
        {
            return View(new PedidoCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var estado = await _context.EstadosPedido.FirstOrDefaultAsync(e => e.NombreEstado == EstadoInicial);
            if (estado == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear el pedido porque falta el estado inicial \"Pendiente\". Avisá al administrador.");
                return View(vm);
            }

            var pedido = new Pedido
            {
                UsuarioId = _userManager.GetUserId(User)!,
                FechaSolicitud = DateTime.Now,
                PrecioTotal = 0,
                Observaciones = vm.Observaciones ?? string.Empty,
                EstadoPedidoId = estado.Id
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Pedido creado. Ahora podés agregar las lámparas y subir tus archivos de referencia.";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

        // ---------------------------------------------------------------- DETALLE
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await ObtenerPedidoPermitidoAsync(id.Value, incluirRelaciones: true);
            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // ---------------------------------------------------------------- EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await ObtenerPedidoPermitidoAsync(id.Value);
            if (pedido == null) return NotFound();

            var vm = new PedidoEditViewModel
            {
                Id = pedido.Id,
                Observaciones = pedido.Observaciones,
                EstadoPedidoId = pedido.EstadoPedidoId
            };
            await CargarEstadosAsync(vm, pedido);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PedidoEditViewModel vm)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(vm.Id);
            if (pedido == null) return NotFound();

            if (EsAdmin && !await _context.EstadosPedido.AnyAsync(e => e.Id == vm.EstadoPedidoId))
            {
                ModelState.AddModelError(nameof(vm.EstadoPedidoId), "El estado elegido no es válido.");
            }

            if (!ModelState.IsValid)
            {
                await CargarEstadosAsync(vm, pedido);
                return View(vm);
            }

            // Solo se modifica lo permitido: el usuario, la fecha y el precio nunca vienen del formulario.
            pedido.Observaciones = vm.Observaciones ?? string.Empty;
            if (EsAdmin)
            {
                pedido.EstadoPedidoId = vm.EstadoPedidoId;
            }

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Los cambios del pedido se guardaron correctamente.";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

        private async Task CargarEstadosAsync(PedidoEditViewModel vm, Pedido pedido)
        {
            vm.PuedeCambiarEstado = EsAdmin;
            vm.EstadoActual = pedido.EstadoPedido?.NombreEstado;
            var estados = await _context.EstadosPedido.OrderBy(e => e.Id).ToListAsync();
            vm.Estados = estados.Select(e => new SelectListItem(e.NombreEstado, e.Id.ToString()));
        }

        // ---------------------------------------------------------------- BORRAR
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await ObtenerPedidoPermitidoAsync(id.Value);
            if (pedido == null) return NotFound();

            if (!PuedeBorrar(pedido))
            {
                TempData["Error"] = "Solo se pueden borrar pedidos que estén en estado \"Pendiente\".";
                return RedirectToAction(nameof(Index));
            }

            return View(pedido);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(id, incluirRelaciones: true);
            if (pedido == null) return NotFound();

            if (!PuedeBorrar(pedido))
            {
                TempData["Error"] = "Solo se pueden borrar pedidos que estén en estado \"Pendiente\".";
                return RedirectToAction(nameof(Index));
            }

            var rutasArchivos = pedido.Archivos
                .Select(a => Path.Combine(_webHostEnvironment.WebRootPath, "adjuntos", Path.GetFileName(a.RutaArchivo)))
                .ToList();

            _context.Pedidos.Remove(pedido); // los detalles y adjuntos se eliminan en cascada
            await _context.SaveChangesAsync();

            foreach (var ruta in rutasArchivos)
            {
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }

            TempData["Exito"] = "El pedido se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- DETALLES (lámparas)
        public async Task<IActionResult> AgregarDetalle(int id)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(id);
            if (pedido == null) return NotFound();

            var vm = new DetalleCreateViewModel { PedidoId = id };
            await CargarListasAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarDetalle(DetalleCreateViewModel vm)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(vm.PedidoId);
            if (pedido == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(vm);
                return View(vm);
            }

            var campana = await _context.MaterialesCampana.FindAsync(vm.MaterialCampanaId!.Value);
            var material = await _context.MaterialesBase.FindAsync(vm.MaterialBaseId!.Value);
            if (campana == null || material == null)
            {
                ModelState.AddModelError(string.Empty, "Alguno de los materiales elegidos ya no existe. Probá de nuevo.");
                await CargarListasAsync(vm);
                return View(vm);
            }

            var detalle = new DetallePedido
            {
                PedidoId = pedido.Id,
                MaterialCampanaId = campana.Id,
                MaterialBaseId = material.Id,
                Cantidad = vm.Cantidad!.Value
            };
            _context.DetallesPedido.Add(detalle);

            // El precio lo calcula siempre el servidor
            pedido.PrecioTotal += campana.PrecioBase * detalle.Cantidad;

            await _context.SaveChangesAsync();

            TempData["Exito"] = "La lámpara se agregó al pedido.";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

        private async Task CargarListasAsync(DetalleCreateViewModel vm)
        {
            var campanas = await _context.MaterialesCampana.OrderBy(c => c.Nombre).ToListAsync();
            var bases = await _context.MaterialesBase.OrderBy(b => b.TipoMaterial).ToListAsync();

            vm.Campanas = campanas.Select(c => new SelectListItem($"{c.Nombre} - $ {c.PrecioBase:0.00}", c.Id.ToString()));
            vm.Bases = bases.Select(b => new SelectListItem(b.TipoMaterial, b.Id.ToString()));
        }

        // ---------------------------------------------------------------- ARCHIVOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirArchivo(int pedidoId, IFormFile? archivoSubido, string? tipoDocumento)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(pedidoId);
            if (pedido == null) return NotFound();

            if (archivoSubido == null || archivoSubido.Length == 0)
            {
                TempData["Error"] = "Seleccioná un archivo para subir.";
                return RedirectToAction(nameof(Details), new { id = pedidoId });
            }

            if (archivoSubido.Length > TamanoMaximoArchivo)
            {
                TempData["Error"] = "El archivo supera el tamaño máximo permitido (5 MB).";
                return RedirectToAction(nameof(Details), new { id = pedidoId });
            }

            var extension = Path.GetExtension(archivoSubido.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                TempData["Error"] = "Formato no permitido. Solo se aceptan archivos JPG, PNG o PDF.";
                return RedirectToAction(nameof(Details), new { id = pedidoId });
            }

            var tipo = tipoDocumento != null && TiposDocumento.Contains(tipoDocumento) ? tipoDocumento : "Referencia";

            var carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "adjuntos");
            Directory.CreateDirectory(carpetaDestino);

            // El nombre en disco lo genera el servidor (nunca se usa el que manda el usuario)
            var nombreUnico = Guid.NewGuid().ToString("N") + extension;
            var rutaFisica = Path.Combine(carpetaDestino, nombreUnico);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivoSubido.CopyToAsync(stream);
            }

            _context.ArchivosAdjuntos.Add(new ArchivoAdjunto
            {
                PedidoId = pedidoId,
                NombreArchivo = Path.GetFileName(archivoSubido.FileName),
                RutaArchivo = "/adjuntos/" + nombreUnico,
                TipoDocumento = tipo,
                FechaSubida = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Exito"] = "El archivo se subió correctamente.";
            return RedirectToAction(nameof(Details), new { id = pedidoId });
        }
    }
}
