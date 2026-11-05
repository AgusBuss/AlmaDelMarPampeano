using System.Globalization;
using System.Text.RegularExpressions;
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
        private const string EstadoCancelado = "Cancelado";
        private const long TamanoMaximoArchivo = 5 * 1024 * 1024; // 5 MB
        private const int MaximoPagosPendientes = 3;              // evita llenar un pedido de comprobantes sin revisar
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".pdf" };
        private static readonly string[] TiposDocumento = { "Referencia", "Comprobante", "Presupuesto" };

        // Montos en formato argentino: 25000, 25000,50 o 25.000,50 (mismo criterio que el formulario)
        private static readonly Regex FormatoMonto = new(@"^(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d{1,2})?$");
        private static readonly CultureInfo CulturaAr = new("es-AR");

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

        // Los comprobantes de pago NO están en wwwroot: no se pueden abrir con solo conocer la dirección.
        // Se entregan por la acción Comprobante, que verifica quién los pide.
        private string CarpetaComprobantes => Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "comprobantes");

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
                    .Include(p => p.Detalles).ThenInclude(d => d.TipoLuz)
                    .Include(p => p.Detalles).ThenInclude(d => d.Lampara).ThenInclude(l => l!.Fotos)
                    .Include(p => p.Archivos)
                    .Include(p => p.Pagos);
            }

            if (!EsAdmin)
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(p => p.UsuarioId == userId);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        // El borrado definitivo es solo del administrador; el cliente cancela (ver acción Cancelar)
        private bool PuedeBorrar(Pedido pedido) => EsAdmin;

        // ---------------------------------------------------------------- DISPONIBILIDAD DE PIEZAS
        // Cada pieza del catálogo es única. Regla: una pieza está disponible solo si no tiene
        // ningún pedido vigente (es decir, ninguno que no esté cancelado). Se recalcula siempre
        // desde los pedidos, así nunca queda desincronizada. Los cambios quedan en la auditoría.
        private async Task ActualizarDisponibilidadAsync(IEnumerable<int> lamparaIds)
        {
            var ids = lamparaIds.Distinct().ToList();
            if (ids.Count == 0) return;

            var cancelado = await _context.EstadosPedido.FirstOrDefaultAsync(e => e.NombreEstado == EstadoCancelado);
            var canceladoId = cancelado?.Id ?? 0;

            foreach (var id in ids)
            {
                var lampara = await _context.Lamparas.FindAsync(id);
                if (lampara == null) continue;

                var tienePedidoVigente = await _context.Pedidos.AnyAsync(p =>
                    p.EstadoPedidoId != canceladoId && p.Detalles.Any(d => d.LamparaId == id));

                lampara.Disponible = !tienePedidoVigente;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<List<int>> LamparasDelPedidoAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Where(p => p.Id == pedidoId)
                .SelectMany(p => p.Detalles)
                .Where(d => d.LamparaId != null)
                .Select(d => d.LamparaId ?? 0)
                .ToListAsync();
        }

        // ---------------------------------------------------------------- LISTADO
        public async Task<IActionResult> Index(int? estadoId)
        {
            var query = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.EstadoPedido)
                .Include(p => p.Detalles).ThenInclude(d => d.Lampara)
                .AsQueryable();

            if (!EsAdmin)
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(p => p.UsuarioId == userId);
            }

            if (estadoId != null)
            {
                query = query.Where(p => p.EstadoPedidoId == estadoId);
            }

            var pedidos = await query.OrderByDescending(p => p.FechaSolicitud).ToListAsync();

            var vm = new PedidosIndexViewModel
            {
                Pedidos = pedidos,
                EsAdmin = EsAdmin,
                EstadoId = estadoId
            };

            if (EsAdmin)
            {
                var ids = pedidos.Select(p => p.UsuarioId).Distinct().ToList();
                vm.Emails = await _userManager.Users
                    .Where(u => ids.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty);
                vm.Estados = await _context.EstadosPedido.AsNoTracking().OrderBy(e => e.Id).ToListAsync();
            }

            return View(vm);
        }

        // ---------------------------------------------------------------- NUEVO PEDIDO (un pedido = una lámpara)
        // Con ?lamparaId=N se pide una pieza del catálogo; sin parámetro, una lámpara a medida.
        public async Task<IActionResult> Pedir(int? lamparaId)
        {
            var vm = new PedidoNuevoViewModel();

            if (lamparaId != null)
            {
                var lampara = await ObtenerLamparaPedibleAsync(lamparaId.Value);
                if (lampara == null)
                {
                    TempData["Error"] = "Esa lámpara no está disponible para pedir. Podés pedir una a medida.";
                    return RedirectToAction(nameof(Pedir));
                }

                vm.LamparaId = lampara.Id;
                vm.Lampara = lampara;
            }

            await CargarOpcionesAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pedir(PedidoNuevoViewModel vm)
        {
            Lampara? lampara = null;

            if (vm.LamparaId != null)
            {
                lampara = await ObtenerLamparaPedibleAsync(vm.LamparaId.Value);
                if (lampara == null)
                {
                    TempData["Error"] = "Esa lámpara ya no está disponible para pedir. Podés pedir una a medida.";
                    return RedirectToAction(nameof(Pedir));
                }

                // Pieza del catálogo: campana y base salen de la pieza y la cantidad es siempre 1.
                // Nada de esto se confía al formulario.
                ModelState.Remove(nameof(vm.MaterialCampanaId));
                ModelState.Remove(nameof(vm.MaterialBaseId));
                ModelState.Remove(nameof(vm.Cantidad));
                ModelState.Remove(nameof(vm.TipoLuzId));
                vm.MaterialCampanaId = lampara.MaterialCampanaId;
                vm.MaterialBaseId = lampara.MaterialBaseId;
                vm.Cantidad = 1;
                vm.Lampara = lampara;
            }

            // El archivo se valida ANTES de crear nada, para no dejar un pedido a medias
            var errorArchivo = ValidarArchivo(vm.Referencia);
            if (errorArchivo != null)
            {
                ModelState.AddModelError(nameof(vm.Referencia), errorArchivo);
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesAsync(vm);
                return View(vm);
            }

            var estado = await _context.EstadosPedido.FirstOrDefaultAsync(e => e.NombreEstado == EstadoInicial);
            var campana = await _context.MaterialesCampana.FindAsync(vm.MaterialCampanaId!.Value);
            var material = await _context.MaterialesBase.FindAsync(vm.MaterialBaseId!.Value);

            // El tipo de luz solo se elige en las lámparas a medida; las piezas del catálogo ya vienen definidas
            TipoLuz? tipoLuz = null;
            if (lampara == null)
            {
                tipoLuz = await _context.TiposLuz.FindAsync(vm.TipoLuzId!.Value);
            }

            if (estado == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear el pedido porque falta el estado inicial \"Pendiente\". Avisá al administrador.");
            }
            else if (campana == null || material == null || (lampara == null && tipoLuz == null))
            {
                ModelState.AddModelError(string.Empty, "Alguna de las opciones elegidas ya no existe. Probá de nuevo.");
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesAsync(vm);
                return View(vm);
            }

            // El precio lo calcula siempre el servidor:
            // pieza del catálogo = el precio que se ve en el catálogo; a medida = (campana + base) x cantidad
            var precioTotal = lampara != null
                ? lampara.Precio
                : (campana!.PrecioBase + material!.Precio) * vm.Cantidad;

            var pedido = new Pedido
            {
                UsuarioId = _userManager.GetUserId(User)!,
                FechaSolicitud = DateTime.Now,
                PrecioTotal = precioTotal,
                Observaciones = vm.Observaciones ?? string.Empty,
                EstadoPedidoId = estado!.Id
            };

            pedido.Detalles.Add(new DetallePedido
            {
                MaterialCampanaId = campana!.Id,
                MaterialBaseId = material!.Id,
                TipoLuzId = tipoLuz?.Id,
                LamparaId = lampara?.Id,
                Cantidad = vm.Cantidad
            });

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // La pieza pedida queda reservada: deja de estar disponible para otros clientes
            if (lampara != null)
            {
                await ActualizarDisponibilidadAsync(new[] { lampara.Id });
            }

            if (vm.Referencia != null && vm.Referencia.Length > 0)
            {
                await GuardarArchivoAsync(pedido.Id, vm.Referencia, "Referencia");
            }

            TempData["Exito"] = "¡Recibimos tu pedido! Te vamos a ir avisando cuando cambie de estado.";
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }

        // Una pieza se puede pedir si está disponible y tiene campana y base definidas
        private async Task<Lampara?> ObtenerLamparaPedibleAsync(int id)
        {
            return await _context.Lamparas
                .AsNoTracking()
                .Include(l => l.Coleccion)
                .Include(l => l.MaterialCampana)
                .Include(l => l.MaterialBase)
                .Include(l => l.Fotos)
                .FirstOrDefaultAsync(l => l.Id == id
                    && l.Disponible
                    && l.MaterialCampanaId != null
                    && l.MaterialBaseId != null);
        }

        private async Task CargarOpcionesAsync(PedidoNuevoViewModel vm)
        {
            vm.CampanasLista = await _context.MaterialesCampana.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
            vm.BasesLista = await _context.MaterialesBase.AsNoTracking().OrderBy(b => b.TipoMaterial).ToListAsync();
            var tipos = await _context.TiposLuz.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
            vm.TiposLuz = tipos.Select(t => new SelectListItem(t.Nombre, t.Id.ToString()));
        }

        // ---------------------------------------------------------------- DETALLE
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await ObtenerPedidoPermitidoAsync(id.Value, incluirRelaciones: true);
            if (pedido == null) return NotFound();

            if (EsAdmin)
            {
                var cliente = await _userManager.FindByIdAsync(pedido.UsuarioId);
                ViewBag.EmailCliente = cliente?.Email;

                var estados = await _context.EstadosPedido.AsNoTracking().OrderBy(e => e.Id).ToListAsync();
                ViewBag.Estados = estados.Select(e =>
                    new SelectListItem(e.NombreEstado, e.Id.ToString(), e.Id == pedido.EstadoPedidoId));
            }

            return View(pedido);
        }

        // ---------------------------------------------------------------- CAMBIAR ESTADO (solo administrador)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id, int estadoPedidoId)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(id);
            if (pedido == null) return NotFound();

            var estado = await _context.EstadosPedido.FindAsync(estadoPedidoId);
            if (estado == null)
            {
                TempData["Error"] = "El estado elegido no es válido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            pedido.EstadoPedidoId = estado.Id;
            await _context.SaveChangesAsync(); // la auditoría queda registrada sola

            // Si el pedido se cancela (o se reactiva), la pieza se libera (o se reserva) sola
            await ActualizarDisponibilidadAsync(await LamparasDelPedidoAsync(id));

            TempData["Exito"] = $"El pedido #{id} pasó a \"{estado.NombreEstado}\".";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- CANCELAR (el pedido queda en estado "Cancelado")
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(id);
            if (pedido == null) return NotFound();

            if (pedido.EstadoPedido?.NombreEstado != EstadoInicial)
            {
                TempData["Error"] = "Solo se pueden cancelar pedidos que estén en estado \"Pendiente\".";
                return RedirectToAction(nameof(Details), new { id });
            }

            var cancelado = await _context.EstadosPedido.FirstOrDefaultAsync(e => e.NombreEstado == EstadoCancelado);
            if (cancelado == null)
            {
                TempData["Error"] = "No se pudo cancelar el pedido. Avisá al administrador.";
                return RedirectToAction(nameof(Details), new { id });
            }

            pedido.EstadoPedidoId = cancelado.Id;
            await _context.SaveChangesAsync(); // queda en la auditoría

            // La pieza reservada vuelve a estar disponible
            await ActualizarDisponibilidadAsync(await LamparasDelPedidoAsync(id));

            TempData["Exito"] = $"Tu pedido #{id} fue cancelado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------- PAGOS
        // El cliente transfiere por fuera del sitio y acá informa el pago subiendo el comprobante.
        // El administrador lo revisa y lo confirma o lo rechaza. Si lo informa el propio
        // administrador (por ejemplo, un pago recibido en mano), queda confirmado directamente.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InformarPago(int pedidoId, string? monto, IFormFile? comprobante)
        {
            var pedido = await ObtenerPedidoPermitidoAsync(pedidoId, incluirRelaciones: true);
            if (pedido == null) return NotFound();

            string? error = null;

            var confirmado = pedido.Pagos.Where(p => p.Estado == EstadosPago.Confirmado).Sum(p => p.Monto);
            var saldo = pedido.PrecioTotal - confirmado;
            var texto = monto?.Trim() ?? string.Empty;
            decimal valor = 0;

            if (pedido.EstadoPedido?.NombreEstado == EstadoCancelado)
            {
                error = "El pedido está cancelado: no se pueden informar pagos.";
            }
            else if (saldo <= 0)
            {
                error = "Este pedido ya está pagado por completo.";
            }
            else if (pedido.Pagos.Count(p => p.Estado == EstadosPago.Pendiente) >= MaximoPagosPendientes)
            {
                error = "Ya hay pagos esperando revisión. Esperá a que el taller los confirme.";
            }
            else if (!FormatoMonto.IsMatch(texto)
                     || !decimal.TryParse(texto, NumberStyles.Number, CulturaAr, out valor)
                     || valor <= 0)
            {
                error = "Ingresá un monto válido, por ejemplo 25000 o 25.000,50.";
            }
            else if (valor > saldo)
            {
                error = $"El monto no puede superar lo que falta pagar ($ {saldo:N2}).";
            }
            else if (comprobante == null || comprobante.Length == 0)
            {
                error = "Subí el comprobante de la transferencia.";
            }
            else
            {
                error = ValidarArchivo(comprobante);
            }

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Details), new { id = pedidoId });
            }

            var nombreOriginal = Path.GetFileName(comprobante!.FileName);
            if (nombreOriginal.Length > 300) nombreOriginal = nombreOriginal[..300];

            _context.Pagos.Add(new Pago
            {
                PedidoId = pedido.Id,
                Monto = valor,
                FechaInformado = DateTime.Now,
                NombreComprobante = nombreOriginal,
                RutaComprobante = await GuardarComprobanteAsync(comprobante), // solo el nombre del archivo en disco
                Estado = EsAdmin ? EstadosPago.Confirmado : EstadosPago.Pendiente
            });
            await _context.SaveChangesAsync();

            TempData["Exito"] = EsAdmin
                ? "El pago se registró y quedó confirmado."
                : "Recibimos tu comprobante. El taller lo va a revisar y confirmar.";
            return RedirectToAction(nameof(Details), new { id = pedidoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ConfirmarPago(int id)
        {
            var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.Id == id);
            if (pago == null) return NotFound();

            if (pago.Estado != EstadosPago.Pendiente)
            {
                TempData["Error"] = "Este pago ya fue revisado.";
                return RedirectToAction(nameof(Details), new { id = pago.PedidoId });
            }

            pago.Estado = EstadosPago.Confirmado;
            pago.MotivoRechazo = null;
            await _context.SaveChangesAsync(); // queda en la auditoría

            TempData["Exito"] = "El pago se confirmó.";
            return RedirectToAction(nameof(Details), new { id = pago.PedidoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RechazarPago(int id, string? motivo)
        {
            var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.Id == id);
            if (pago == null) return NotFound();

            if (pago.Estado != EstadosPago.Pendiente)
            {
                TempData["Error"] = "Este pago ya fue revisado.";
                return RedirectToAction(nameof(Details), new { id = pago.PedidoId });
            }

            var texto = motivo?.Trim() ?? string.Empty;
            if (texto.Length == 0 || texto.Length > 300)
            {
                TempData["Error"] = "Indicá el motivo del rechazo (hasta 300 caracteres).";
                return RedirectToAction(nameof(Details), new { id = pago.PedidoId });
            }

            pago.Estado = EstadosPago.Rechazado;
            pago.MotivoRechazo = texto;
            await _context.SaveChangesAsync(); // queda en la auditoría

            TempData["Exito"] = "El pago se rechazó. El cliente va a ver el motivo.";
            return RedirectToAction(nameof(Details), new { id = pago.PedidoId });
        }

        // Entrega el comprobante solo a su dueño o al administrador (la carpeta no es pública)
        [HttpGet]
        public async Task<IActionResult> Comprobante(int id)
        {
            var pago = await _context.Pagos.Include(p => p.Pedido).FirstOrDefaultAsync(p => p.Id == id);
            if (pago == null) return NotFound();

            if (!EsAdmin && pago.Pedido?.UsuarioId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var ruta = Path.Combine(CarpetaComprobantes, Path.GetFileName(pago.RutaComprobante));
            if (!System.IO.File.Exists(ruta)) return NotFound();

            var tipo = Path.GetExtension(ruta).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                _ => "image/jpeg"
            };
            return PhysicalFile(ruta, tipo);
        }

        private async Task<string> GuardarComprobanteAsync(IFormFile archivo)
        {
            Directory.CreateDirectory(CarpetaComprobantes);

            // El nombre en disco lo genera el servidor (nunca se usa el que manda el usuario)
            var nombre = Guid.NewGuid().ToString("N") + Path.GetExtension(archivo.FileName).ToLowerInvariant();
            using (var stream = new FileStream(Path.Combine(CarpetaComprobantes, nombre), FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }
            return nombre;
        }

        // ---------------------------------------------------------------- EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await ObtenerPedidoPermitidoAsync(id.Value);
            if (pedido == null) return NotFound();

            if (!EsAdmin && pedido.EstadoPedido?.NombreEstado != EstadoInicial)
            {
                TempData["Error"] = "Solo podés editar tus notas mientras el pedido esté en estado \"Pendiente\".";
                return RedirectToAction(nameof(Details), new { id = pedido.Id });
            }

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

            if (!EsAdmin && pedido.EstadoPedido?.NombreEstado != EstadoInicial)
            {
                TempData["Error"] = "Solo podés editar tus notas mientras el pedido esté en estado \"Pendiente\".";
                return RedirectToAction(nameof(Details), new { id = pedido.Id });
            }

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

            // El administrador también puede cambiar el estado desde acá: se recalcula la disponibilidad
            if (EsAdmin)
            {
                await ActualizarDisponibilidadAsync(await LamparasDelPedidoAsync(pedido.Id));
            }

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

        // ---------------------------------------------------------------- BORRAR (solo administrador)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await ObtenerPedidoPermitidoAsync(id.Value);
            if (pedido == null) return NotFound();

            if (!PuedeBorrar(pedido))
            {
                TempData["Error"] = "Solo el administrador puede eliminar pedidos. Si todavía está pendiente, podés cancelarlo.";
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
                TempData["Error"] = "Solo el administrador puede eliminar pedidos. Si todavía está pendiente, podés cancelarlo.";
                return RedirectToAction(nameof(Index));
            }

            var rutasArchivos = pedido.Archivos
                .Select(a => Path.Combine(_webHostEnvironment.WebRootPath, "adjuntos", Path.GetFileName(a.RutaArchivo)))
                .ToList();

            var rutasComprobantes = pedido.Pagos
                .Select(p => Path.Combine(CarpetaComprobantes, Path.GetFileName(p.RutaComprobante)))
                .ToList();

            // Se guardan las piezas del pedido antes de borrarlo, para liberarlas después
            var lamparaIds = pedido.Detalles
                .Where(d => d.LamparaId != null)
                .Select(d => d.LamparaId!.Value)
                .ToList();

            _context.Pedidos.Remove(pedido); // los detalles, adjuntos y pagos se eliminan en cascada
            await _context.SaveChangesAsync();

            await ActualizarDisponibilidadAsync(lamparaIds);

            foreach (var ruta in rutasArchivos.Concat(rutasComprobantes))
            {
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }

            TempData["Exito"] = "El pedido se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
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

            var errorArchivo = ValidarArchivo(archivoSubido);
            if (errorArchivo != null)
            {
                TempData["Error"] = errorArchivo;
                return RedirectToAction(nameof(Details), new { id = pedidoId });
            }

            var tipo = tipoDocumento != null && TiposDocumento.Contains(tipoDocumento) ? tipoDocumento : "Referencia";
            await GuardarArchivoAsync(pedidoId, archivoSubido, tipo);

            TempData["Exito"] = "El archivo se subió correctamente.";
            return RedirectToAction(nameof(Details), new { id = pedidoId });
        }

        // Devuelve el mensaje de error, o null si el archivo es válido (o si no hay archivo)
        private static string? ValidarArchivo(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0) return null;

            if (archivo.Length > TamanoMaximoArchivo)
            {
                return "El archivo supera el tamaño máximo permitido (5 MB).";
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return "Formato no permitido. Solo se aceptan archivos JPG, PNG o PDF.";
            }

            return null;
        }

        // Guarda el archivo en disco y su registro en la base. Asume que ya fue validado.
        private async Task GuardarArchivoAsync(int pedidoId, IFormFile archivo, string tipoDocumento)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            var carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "adjuntos");
            Directory.CreateDirectory(carpetaDestino);

            // El nombre en disco lo genera el servidor (nunca se usa el que manda el usuario)
            var nombreUnico = Guid.NewGuid().ToString("N") + extension;
            var rutaFisica = Path.Combine(carpetaDestino, nombreUnico);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            _context.ArchivosAdjuntos.Add(new ArchivoAdjunto
            {
                PedidoId = pedidoId,
                NombreArchivo = Path.GetFileName(archivo.FileName),
                RutaArchivo = "/adjuntos/" + nombreUnico,
                TipoDocumento = tipoDocumento,
                FechaSubida = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}