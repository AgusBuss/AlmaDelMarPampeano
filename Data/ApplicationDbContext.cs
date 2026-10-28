using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AlmaDelMarPampeano.Models;

namespace AlmaDelMarPampeano.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Nuestras 7 tablas de dominio
        public DbSet<MaterialCampana> MaterialesCampana { get; set; }
        public DbSet<MaterialBase> MaterialesBase { get; set; }
        public DbSet<EstadoPedido> EstadosPedido { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
    }
}