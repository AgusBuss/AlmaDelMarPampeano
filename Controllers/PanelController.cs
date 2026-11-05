using System.Text;
using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.Services;
using AlmaDelMarPampeano.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AlmaDelMarPampeano.Controllers
{
    // Panel de estadísticas y reporte con IA: solo para el administrador
    [Authorize(Roles = "Administrador")]
    public class PanelController : Controller
    {
        private const string EstadoCancelado = "Cancelado";

        private readonly ApplicationDbContext _context;
        private readonly GeminiService _gemini;

        public PanelController(ApplicationDbContext context, GeminiService gemini)
        {
            _context = context;
            _gemini = gemini;
        }

        public async Task<IActionResult> Index(int dias = 30)
        {
            return View(await ConstruirAsync(dias));
        }

        // Le pide a la IA un informe en lenguaje natural con los números del período.
        // Usa el mismo límite de consultas por minuto que el chat (protege la cuota gratuita).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("chat")]
        public async Task<IActionResult> Reporte(int dias = 30)
        {
            var vm = await ConstruirAsync(dias);

            if (vm.TotalPedidos == 0 && vm.TotalOperaciones == 0)
            {
                vm.Reporte = $"No hay pedidos ni actividad registrada en {vm.Periodo}, así que no hay nada para resumir.";
                return View("Index", vm);
            }

            const string instrucciones =
                "Sos el analista de \"Alma del Mar Pampeano\", un taller artesanal de lámparas de piedra y madera de La Pampa. " +
                "Recibís los números de un período y escribís un informe ejecutivo para la dueña del taller. " +
                "Escribí en español rioplatense, con tono claro y profesional, en 2 o 3 párrafos cortos (entre 6 y 9 oraciones en total). " +
                "Contá cómo anduvieron los pedidos, las ventas y los cobros, qué colecciones y campanas se eligieron más, " +
                "y cerrá con una o dos cosas concretas para atender (por ejemplo pagos sin revisar o piezas no disponibles). " +
                "Usá SOLO los datos que te paso: no inventes cifras, comparaciones con otros períodos ni causas. " +
                "Si un dato es cero o falta, decilo sin dramatizar. No uses viñetas, títulos ni asteriscos: solo párrafos.";

            var conversacion = new List<MensajeChat> { new("user", ConstruirTextoDatos(vm)) };
            var respuesta = await _gemini.ResponderAsync(instrucciones, conversacion, HttpContext.RequestAborted);

            // La IA a veces devuelve negritas con asteriscos; se sacan para mostrar texto limpio
            vm.Reporte = respuesta.Replace("**", string.Empty).Trim();
            return View("Index", vm);
        }

        // ---------------------------------------------------------------- DATOS
        private async Task<PanelViewModel> ConstruirAsync(int dias)
        {
            if (dias != 0 && dias != 7 && dias != 30 && dias != 90) dias = 30; // 0 = todo el historial
            DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : null;

            var estados = await _context.EstadosPedido.AsNoTracking().OrderBy(e => e.Id).ToListAsync();

            var consulta = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.EstadoPedido)
                .Include(p => p.Detalles).ThenInclude(d => d.MaterialCampana)
                .Include(p => p.Detalles).ThenInclude(d => d.Lampara).ThenInclude(l => l!.Coleccion)
                .AsQueryable();

            if (desde != null) consulta = consulta.Where(p => p.FechaSolicitud >= desde);

            var pedidos = await consulta.ToListAsync();
            var vigentes = pedidos.Where(p => p.EstadoPedido?.NombreEstado != EstadoCancelado).ToList();

            var vm = new PanelViewModel
            {
                Dias = dias,
                TotalPedidos = pedidos.Count,
                VentasEstimadas = vigentes.Sum(p => p.PrecioTotal)
            };

            vm.PorEstado = estados.Select(e => new ConteoItem
            {
                Nombre = e.NombreEstado,
                Cantidad = pedidos.Count(p => p.EstadoPedidoId == e.Id),
                Clase = ClaseEstado(e.NombreEstado)
            }).ToList();

            vm.PedidosMedida = vigentes.Count(p => p.Detalles.Any(d => d.LamparaId == null));
            vm.PedidosCatalogo = vigentes.Count - vm.PedidosMedida;

            vm.ColeccionesTop = vigentes
                .SelectMany(p => p.Detalles)
                .Where(d => d.Lampara?.Coleccion != null)
                .GroupBy(d => d.Lampara!.Coleccion!.Nombre)
                .Select(g => new ConteoItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(c => c.Cantidad).ThenBy(c => c.Nombre)
                .Take(3).ToList();

            vm.CampanasTop = vigentes
                .SelectMany(p => p.Detalles)
                .Where(d => d.MaterialCampana != null)
                .GroupBy(d => d.MaterialCampana!.Nombre)
                .Select(g => new ConteoItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(c => c.Cantidad).ThenBy(c => c.Nombre)
                .Take(3).ToList();

            // Dinero: lo cobrado es solo lo que el administrador confirmó
            var pagos = _context.Pagos.AsNoTracking().Where(p => p.Estado == EstadosPago.Confirmado);
            if (desde != null) pagos = pagos.Where(p => p.FechaInformado >= desde);
            vm.Cobrado = await pagos.SumAsync(p => p.Monto);
            vm.PagosPorRevisar = await _context.Pagos.CountAsync(p => p.Estado == EstadosPago.Pendiente);

            vm.LamparasDisponibles = await _context.Lamparas.CountAsync(l => l.Disponible);
            vm.LamparasNoDisponibles = await _context.Lamparas.CountAsync(l => !l.Disponible);

            // Actividad del sistema, a partir de la auditoría
            var auditoria = _context.Auditorias.AsNoTracking().AsQueryable();
            if (desde != null) auditoria = auditoria.Where(a => a.FechaHora >= desde);

            vm.TotalOperaciones = await auditoria.CountAsync();
            vm.UsuariosActivos = await auditoria.Select(a => a.UsuarioId).Distinct().CountAsync();

            var porTipo = await auditoria
                .GroupBy(a => new { a.TipoAccion, a.Entidad })
                .Select(g => new { g.Key.TipoAccion, g.Key.Entidad, Cantidad = g.Count() })
                .ToListAsync();

            vm.Actividad = porTipo
                .OrderByDescending(x => x.Cantidad)
                .Take(6)
                .Select(x => new ConteoItem { Nombre = $"{x.TipoAccion ?? "—"} · {x.Entidad ?? "—"}", Cantidad = x.Cantidad })
                .ToList();

            var ultimas = await auditoria.OrderByDescending(a => a.FechaHora).Take(8).ToListAsync();
            vm.UltimasAcciones = ultimas
                .Select(a => $"{a.FechaHora:dd/MM HH:mm} · {a.UsuarioEmail} · {a.AccionRealizada}")
                .ToList();

            return vm;
        }

        // Texto con los números que se le pasan a la IA. Nunca lleva correos ni datos de clientes.
        private static string ConstruirTextoDatos(PanelViewModel vm)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Período analizado: {vm.Periodo}.");
            sb.AppendLine($"Pedidos recibidos: {vm.TotalPedidos}.");
            sb.AppendLine("Pedidos por estado: " + string.Join(", ", vm.PorEstado.Select(e => $"{e.Nombre} {e.Cantidad}")) + ".");
            sb.AppendLine($"Pedidos vigentes de piezas del catálogo: {vm.PedidosCatalogo}. Pedidos vigentes a medida: {vm.PedidosMedida}.");
            sb.AppendLine($"Ventas estimadas (pedidos no cancelados): $ {vm.VentasEstimadas:N2}.");
            sb.AppendLine($"Cobrado y confirmado en el período: $ {vm.Cobrado:N2}.");
            sb.AppendLine($"Pagos esperando revisión en este momento: {vm.PagosPorRevisar}.");
            sb.AppendLine($"Lámparas del catálogo: {vm.LamparasDisponibles} disponibles y {vm.LamparasNoDisponibles} no disponibles (reservadas o vendidas).");
            sb.AppendLine("Colecciones más pedidas: " + (vm.ColeccionesTop.Any() ? string.Join(", ", vm.ColeccionesTop.Select(c => $"{c.Nombre} ({c.Cantidad})")) : "sin datos") + ".");
            sb.AppendLine("Campanas más elegidas: " + (vm.CampanasTop.Any() ? string.Join(", ", vm.CampanasTop.Select(c => $"{c.Nombre} ({c.Cantidad})")) : "sin datos") + ".");
            sb.AppendLine($"Actividad en el sistema: {vm.TotalOperaciones} operaciones registradas hechas por {vm.UsuariosActivos} usuarios. " +
                          "Las más frecuentes: " + (vm.Actividad.Any() ? string.Join(", ", vm.Actividad.Select(a => $"{a.Nombre} ({a.Cantidad})")) : "sin datos") + ".");
            return sb.ToString();
        }

        // Mismos colores que las etiquetas de estado del resto del sitio
        private static string ClaseEstado(string nombre)
        {
            var n = nombre.ToLowerInvariant();
            if (n.Contains("cancel")) return "estado-cancelado";
            if (n.Contains("entreg")) return "estado-entregado";
            if (n.Contains("termin")) return "estado-terminado";
            if (n.Contains("produc")) return "estado-produccion";
            return "estado-pendiente";
        }
    }
}