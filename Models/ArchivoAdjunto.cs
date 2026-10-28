using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    public class ArchivoAdjunto
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }

        [Required]
        public string NombreArchivo { get; set; }

        [Required]
        public string RutaArchivo { get; set; }

        public string TipoDocumento { get; set; } // Ej: "Referencia", "Presupuesto", "Comprobante"

        public DateTime FechaSubida { get; set; }
    }
}