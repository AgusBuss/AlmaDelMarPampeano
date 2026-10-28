using System;
using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        public string UsuarioId { get; set; }

        [Required]
        public string AccionRealizada { get; set; }

        public DateTime FechaHora { get; set; }
    }
}