using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class Coleccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la colección es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [StringLength(300)]
        [Display(Name = "Cuaderno (PDF)")]
        public string? RutaCuaderno { get; set; }
    }
}