using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Total")]
        public decimal PrecioTotal { get; set; }

        public string Observaciones { get; set; }

        // Relación con el Usuario de Identity (su ID es un string)
        public string UsuarioId { get; set; }

        public int EstadoPedidoId { get; set; }
        [ForeignKey("EstadoPedidoId")]
        public EstadoPedido EstadoPedido { get; set; }

        // Relaciones 1 a muchos
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
        public List<ArchivoAdjunto> Archivos { get; set; } = new List<ArchivoAdjunto>();
    }
}