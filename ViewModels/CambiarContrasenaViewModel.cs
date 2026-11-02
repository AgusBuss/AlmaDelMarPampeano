using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.ViewModels
{
    public class CambiarContrasenaViewModel
    {
        [Required(ErrorMessage = "Ingresá tu contraseña actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá la contraseña nueva.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña nueva debe tener al menos {2} caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string ContrasenaNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Repetí la contraseña nueva.")]
        [DataType(DataType.Password)]
        [Compare(nameof(ContrasenaNueva), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Repetir contraseña nueva")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}