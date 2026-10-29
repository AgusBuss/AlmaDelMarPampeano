using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    public class MaterialCampana
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del material es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La historia de origen es obligatoria")]
        [Display(Name = "Historia de origen")]
        public string HistoriaOrigen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "El precio base es obligatorio")]
        [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor a cero")]
        [Display(Name = "Precio base")]
        public decimal PrecioBase { get; set; }
    }
}