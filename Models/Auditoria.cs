using System;
using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        // Quién realizó la operación
        public string UsuarioId { get; set; }
        public string? UsuarioEmail { get; set; }

        // Qué hizo: texto legible, por ejemplo "Creó Pedido #5"
        [Required]
        public string AccionRealizada { get; set; }

        // Datos para filtrar: "Crear", "Modificar" o "Eliminar" / nombre de la entidad / su Id
        public string? TipoAccion { get; set; }
        public string? Entidad { get; set; }
        public string? EntidadId { get; set; }

        public DateTime FechaHora { get; set; }
    }
}