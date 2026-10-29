using System.Security.Claims;
using AlmaDelMarPampeano.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AlmaDelMarPampeano.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Nuestras 7 tablas de dominio
        public DbSet<MaterialCampana> MaterialesCampana { get; set; }
        public DbSet<MaterialBase> MaterialesBase { get; set; }
        public DbSet<EstadoPedido> EstadosPedido { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<TipoLuz> TiposLuz { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Los datos de catálogo en uso no se pueden borrar (evita borrados en cascada accidentales).
            // Para borrarlos primero hay que quitarlos de los pedidos que los usan.
            builder.Entity<DetallePedido>()
                .HasOne(d => d.MaterialCampana)
                .WithMany()
                .HasForeignKey(d => d.MaterialCampanaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DetallePedido>()
                .HasOne(d => d.MaterialBase)
                .WithMany()
                .HasForeignKey(d => d.MaterialBaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Pedido>()
                .HasOne(p => p.EstadoPedido)
                .WithMany()
                .HasForeignKey(p => p.EstadoPedidoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DetallePedido>()
                .HasOne(d => d.TipoLuz)
                .WithMany()
                .HasForeignKey(d => d.TipoLuzId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // ------------------------------------------------------------------
        // AUDITORÍA AUTOMÁTICA
        // Cada vez que un usuario logueado crea, modifica o elimina una de estas
        // entidades, se guarda quién fue, qué hizo y cuándo. No hace falta tocar
        // los controladores: todo pasa por SaveChangesAsync.
        // ------------------------------------------------------------------
        private static readonly Dictionary<Type, string> EntidadesAuditadas = new()
        {
            { typeof(Pedido), "Pedido" },
            { typeof(DetallePedido), "Detalle de pedido" },
            { typeof(ArchivoAdjunto), "Archivo adjunto" },
            { typeof(MaterialCampana), "Campana" },
            { typeof(MaterialBase), "Base" },
            { typeof(EstadoPedido), "Estado de pedido" },
            { typeof(TipoLuz), "Tipo de luz" }
        };

        private sealed record CambioPendiente(
            EntityEntry Entrada, string TipoAccion, string Entidad, string? IdAntes, string? Campos);

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var usuario = _httpContextAccessor.HttpContext?.User;

            // Sin usuario logueado (arranque de la app, datos iniciales) no se audita
            if (usuario?.Identity?.IsAuthenticated != true)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            ChangeTracker.DetectChanges();

            var pendientes = ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added
                          || e.State == EntityState.Modified
                          || e.State == EntityState.Deleted)
                         && EntidadesAuditadas.ContainsKey(e.Entity.GetType()))
                .Select(e => new CambioPendiente(
                    e,
                    e.State == EntityState.Added ? "Crear" : e.State == EntityState.Modified ? "Modificar" : "Eliminar",
                    EntidadesAuditadas[e.Entity.GetType()],
                    e.State == EntityState.Added ? null : e.Property("Id").CurrentValue?.ToString(),
                    e.State == EntityState.Modified
                        ? string.Join(", ", e.Properties
                            .Where(p => p.IsModified && !p.Metadata.IsPrimaryKey())
                            .Select(p => p.Metadata.Name))
                        : null))
                .Where(c => c.TipoAccion != "Modificar" || !string.IsNullOrEmpty(c.Campos))
                .ToList();

            var resultado = await base.SaveChangesAsync(cancellationToken);

            if (pendientes.Count > 0)
            {
                var usuarioId = usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                var email = usuario.Identity?.Name;

                foreach (var cambio in pendientes)
                {
                    // Para los registros nuevos, el Id recién existe después de guardar
                    var id = cambio.IdAntes ?? cambio.Entrada.Property("Id").CurrentValue?.ToString();

                    var texto = cambio.TipoAccion switch
                    {
                        "Crear" => $"Creó {cambio.Entidad} #{id}",
                        "Modificar" => $"Modificó {cambio.Entidad} #{id} ({cambio.Campos})",
                        _ => $"Eliminó {cambio.Entidad} #{id}"
                    };

                    Auditorias.Add(new Auditoria
                    {
                        UsuarioId = usuarioId,
                        UsuarioEmail = email,
                        AccionRealizada = texto,
                        TipoAccion = cambio.TipoAccion,
                        Entidad = cambio.Entidad,
                        EntidadId = id,
                        FechaHora = DateTime.Now
                    });
                }

                await base.SaveChangesAsync(cancellationToken);
            }

            return resultado;
        }
    }
}