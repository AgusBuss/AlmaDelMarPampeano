using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Precio fijo de este tipo de base. Se suma al precio de la campana en cada lámpara.
        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor a cero")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }
    }
}