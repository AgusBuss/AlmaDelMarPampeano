using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    public class Lampara
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "N.º de pieza")]
        [Range(1, 100000, ErrorMessage = "El número de pieza debe ser mayor a 0.")]
        public int? NumeroPieza { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        [Display(Name = "Colección")]
        public int? ColeccionId { get; set; }

        [ForeignKey(nameof(ColeccionId))]
        public Coleccion? Coleccion { get; set; }

        [Display(Name = "Campana")]
        public int? MaterialCampanaId { get; set; }

        [ForeignKey(nameof(MaterialCampanaId))]
        public MaterialCampana? MaterialCampana { get; set; }

        [Display(Name = "Base")]
        public int? MaterialBaseId { get; set; }

        [ForeignKey(nameof(MaterialBaseId))]
        public MaterialBase? MaterialBase { get; set; }
        public ICollection<FotoLampara> Fotos { get; set; } = new List<FotoLampara>();
    }
}