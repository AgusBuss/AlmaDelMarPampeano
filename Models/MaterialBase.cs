using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class MaterialBase
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El material de la base es obligatorio")]
        [Display(Name = "Tipo de material")]
        public string TipoMaterial { get; set; } // Ej: Caldén Natural, Piedra Cuarzo, Base de Yeso

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }
}