using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class TipoLuz
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de luz es obligatorio")]
        [StringLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres")]
        [Display(Name = "Tipo de luz")]
        public string Nombre { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }
    }
}