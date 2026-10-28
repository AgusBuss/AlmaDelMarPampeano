using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class MaterialBase
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El material de la base es obligatorio")]
        public string TipoMaterial { get; set; } // Ej: Caldén Natural, Piedra Cuarzo, Base de Yeso

        public string Descripcion { get; set; }
    }
}