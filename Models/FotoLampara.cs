using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.Models
{
    public class FotoLampara
    {
        public int Id { get; set; }

        public int LamparaId { get; set; }
        public Lampara? Lampara { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Archivo")]
        public string RutaArchivo { get; set; } = string.Empty;

        // Posición en la pasarela: la de menor orden es la portada
        [Display(Name = "Orden")]
        public int Orden { get; set; }
    }
}