using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class EstadoPedido
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del estado es obligatorio")]
        [Display(Name = "Nombre del estado")]
        public string NombreEstado { get; set; }
    }
}