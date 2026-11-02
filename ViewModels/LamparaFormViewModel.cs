using System.ComponentModel.DataAnnotations;
using AlmaDelMarPampeano.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlmaDelMarPampeano.ViewModels
{
    public class LamparaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Range(1, 100000, ErrorMessage = "El número de pieza debe ser mayor a 0.")]
        [Display(Name = "N.º de pieza")]
        public int? NumeroPieza { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Display(Name = "Precio")]
        public decimal? Precio { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        [Required(ErrorMessage = "Elegí la colección de la lámpara.")]
        [Display(Name = "Colección")]
        public int? ColeccionId { get; set; }

        [Required(ErrorMessage = "Elegí la campana de la lámpara.")]
        [Display(Name = "Campana")]
        public int? MaterialCampanaId { get; set; }

        [Required(ErrorMessage = "Elegí la base de la lámpara.")]
        [Display(Name = "Base")]
        public int? MaterialBaseId { get; set; }

        // Fotos nuevas que se suben (puede ser más de una)
        [Display(Name = "Fotos")]
        public List<IFormFile>? Imagenes { get; set; }

        // Al editar: ids de las fotos actuales que el administrador marcó para quitar
        public List<int>? FotosAEliminar { get; set; }

        // Solo para mostrar las fotos actuales al editar
        public List<FotoLampara> FotosActuales { get; set; } = new();

        public IEnumerable<SelectListItem> Colecciones { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Campanas { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Bases { get; set; } = new List<SelectListItem>();
    }
}