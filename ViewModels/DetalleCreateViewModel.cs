using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlmaDelMarPampeano.ViewModels
{
    public class DetalleCreateViewModel
    {
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "Elegí una campana.")]
        [Display(Name = "Campana")]
        public int? MaterialCampanaId { get; set; }

        [Required(ErrorMessage = "Elegí un material para la base.")]
        [Display(Name = "Base")]
        public int? MaterialBaseId { get; set; }

        [Required(ErrorMessage = "Elegí el tipo de luz.")]
        [Display(Name = "Tipo de luz")]
        public int? TipoLuzId { get; set; }

        [Required(ErrorMessage = "Ingresá la cantidad.")]
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
        [Display(Name = "Cantidad")]
        public int? Cantidad { get; set; }

        public IEnumerable<SelectListItem>? Campanas { get; set; }
        public IEnumerable<SelectListItem>? Bases { get; set; }
        public IEnumerable<SelectListItem>? TiposLuz { get; set; }
    }
}