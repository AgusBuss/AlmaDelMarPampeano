using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    // Un pago informado por el cliente (seña o saldo), con su comprobante.
    // El administrador lo revisa y lo confirma o lo rechaza.
    public class Pago
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaInformado { get; set; }

        [Required, StringLength(300)]
        public string NombreComprobante { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string RutaComprobante { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Estado { get; set; } = EstadosPago.Pendiente;

        [StringLength(300)]
        public string? MotivoRechazo { get; set; }
    }

    // Estados posibles de un pago (un solo lugar para no repetir textos en el código)
    public static class EstadosPago
    {
        public const string Pendiente = "Pendiente de revisión";
        public const string Confirmado = "Confirmado";
        public const string Rechazado = "Rechazado";
    }
}