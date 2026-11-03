using System.ComponentModel.DataAnnotations;
using AlmaDelMarPampeano.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlmaDelMarPampeano.ViewModels
{
    // Formulario único para pedir una lámpara (de catálogo o a medida)
    public class PedidoNuevoViewModel
    {
        // Si viene con valor, el pedido es de una pieza del catálogo
        public int? LamparaId { get; set; }

        [Required(ErrorMessage = "Elegí una campana.")]
        [Display(Name = "Campana")]
        public int? MaterialCampanaId { get; set; }

        [Required(ErrorMessage = "Elegí una base.")]
        [Display(Name = "Base")]
        public int? MaterialBaseId { get; set; }

        [Required(ErrorMessage = "Elegí el tipo de luz.")]
        [Display(Name = "Tipo de luz")]
        public int? TipoLuzId { get; set; }

        [Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        [Display(Name = "Contanos qué estás buscando")]
        public string? Observaciones { get; set; }

        [Display(Name = "Imagen de referencia (opcional)")]
        public IFormFile? Referencia { get; set; }

        // ---- Solo para mostrar en pantalla (no vienen del formulario) ----
        public Lampara? Lampara { get; set; }
        public List<MaterialCampana> CampanasLista { get; set; } = new();
        public List<MaterialBase> BasesLista { get; set; } = new();
        public IEnumerable<SelectListItem> TiposLuz { get; set; } = new List<SelectListItem>();
    }
}