using System.ComponentModel.DataAnnotations;

namespace AlmaDelMarPampeano.ViewModels
{
    // Solo los datos que el cliente puede cargar al iniciar un pedido.
    // El usuario, la fecha, el precio y el estado los pone el servidor.
    public class PedidoCreateViewModel
    {
        [Display(Name = "Observaciones (opcional)")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }
    }
}
