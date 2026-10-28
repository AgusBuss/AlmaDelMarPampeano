using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class EstadoPedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreEstado { get; set; }
    }
}