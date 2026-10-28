using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;

namespace AlmaDelMarPampeano.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment; // Permite acceder a wwwroot para guardar archivos

        public PedidosController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Administrador");
            var pedidosQuery = _context.Pedidos.Include(p => p.EstadoPedido).AsQueryable();

            if (!isAdmin)
            {
                var userId = _userManager.GetUserId(User);
                pedidosQuery = pedidosQuery.Where(p => p.UsuarioId == userId);
            }

            return View(await pedidosQuery.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["EstadoPedidoId"] = new SelectList(_context.EstadosPedido, "Id", "NombreEstado");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Observaciones,EstadoPedidoId")] Pedido pedido)
        {
            var userId = _userManager.GetUserId(User);
            pedido.UsuarioId = userId;
            pedido.FechaSolicitud = DateTime.Now;
            pedido.PrecioTotal = 0;

            ModelState.Remove("UsuarioId");
            ModelState.Remove("EstadoPedido");

            if (ModelState.IsValid)
            {
                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EstadoPedidoId"] = new SelectList(_context.EstadosPedido, "Id", "NombreEstado", pedido.EstadoPedidoId);
            return View(pedido);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.EstadoPedido)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.MaterialCampana)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.MaterialBase)
                .Include(p => p.Archivos) // Incluimos los archivos para verlos en la vista
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            ViewData["EstadoPedidoId"] = new SelectList(_context.EstadosPedido, "Id", "NombreEstado", pedido.EstadoPedidoId);
            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsuarioId,FechaSolicitud,PrecioTotal,Observaciones,EstadoPedidoId")] Pedido pedido)
        {
            if (id != pedido.Id) return NotFound();

            ModelState.Remove("EstadoPedido");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EstadoPedidoId"] = new SelectList(_context.EstadosPedido, "Id", "NombreEstado", pedido.EstadoPedidoId);
            return View(pedido);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.EstadoPedido)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null) return NotFound();

            return View(pedido);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }

        public IActionResult AgregarDetalle(int id)
        {
            ViewData["PedidoId"] = id;
            ViewData["MaterialCampanaId"] = new SelectList(_context.MaterialesCampana, "Id", "Nombre");
            ViewData["MaterialBaseId"] = new SelectList(_context.MaterialesBase, "Id", "TipoMaterial");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarDetalle([Bind("PedidoId,MaterialCampanaId,MaterialBaseId,Cantidad")] DetallePedido detalle)
        {
            ModelState.Remove("Pedido");
            ModelState.Remove("MaterialCampana");
            ModelState.Remove("MaterialBase");

            if (ModelState.IsValid)
            {
                _context.Add(detalle);

                var pedido = await _context.Pedidos.FindAsync(detalle.PedidoId);
                var campana = await _context.MaterialesCampana.FindAsync(detalle.MaterialCampanaId);

                if (pedido != null && campana != null)
                {
                    decimal subtotal = campana.PrecioBase * detalle.Cantidad;
                    pedido.PrecioTotal += subtotal;
                    _context.Update(pedido);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = detalle.PedidoId });
            }

            ViewData["PedidoId"] = detalle.PedidoId;
            ViewData["MaterialCampanaId"] = new SelectList(_context.MaterialesCampana, "Id", "Nombre", detalle.MaterialCampanaId);
            ViewData["MaterialBaseId"] = new SelectList(_context.MaterialesBase, "Id", "TipoMaterial", detalle.MaterialBaseId);
            return View(detalle);
        }

        // LÓGICA NUEVA: Subir Archivos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirArchivo(int pedidoId, IFormFile archivoSubido, string tipoDocumento)
        {
            if (archivoSubido != null && archivoSubido.Length > 0)
            {
                // 1. Definir la ruta física en wwwroot/adjuntos
                string carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "adjuntos");

                if (!Directory.Exists(carpetaDestino))
                {
                    Directory.CreateDirectory(carpetaDestino);
                }

                // 2. Crear nombre único para evitar sobreescribir archivos con el mismo nombre
                string nombreUnico = Guid.NewGuid().ToString() + "_" + archivoSubido.FileName;
                string rutaFisica = Path.Combine(carpetaDestino, nombreUnico);

                // 3. Guardar físicamente
                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await archivoSubido.CopyToAsync(stream);
                }

                // 4. Guardar en base de datos
                var nuevoAdjunto = new ArchivoAdjunto
                {
                    PedidoId = pedidoId,
                    NombreArchivo = archivoSubido.FileName,
                    RutaArchivo = "/adjuntos/" + nombreUnico,
                    TipoDocumento = string.IsNullOrEmpty(tipoDocumento) ? "Referencia" : tipoDocumento,
                    FechaSubida = DateTime.Now
                };

                _context.ArchivosAdjuntos.Add(nuevoAdjunto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = pedidoId });
        }
    }
}