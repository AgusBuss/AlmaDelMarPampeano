using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    public class MaterialCampana
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del material es obligatorio")]
        public string Nombre { get; set; }

        public string HistoriaOrigen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "El precio base es obligatorio")]
        public decimal PrecioBase { get; set; }
    }
}