using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.ViewModels
{
    // Una fila de la lista de usuarios que ve el administrador
    public class UsuarioListaViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    // Formulario para asignar una contraseña temporal
    public class RestablecerContrasenaViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá una contraseña temporal.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
        [Display(Name = "Contraseña temporal")]
        public string ContrasenaTemporal { get; set; } = string.Empty;
    }
}