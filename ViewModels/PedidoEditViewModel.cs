using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlmaDelMarPampeano.ViewModels
{
    public class PedidoEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        // Solo lo respeta el servidor si el usuario es Administrador
        [Display(Name = "Estado del pedido")]
        public int EstadoPedidoId { get; set; }

        // Datos de apoyo para la vista (no se reciben del formulario)
        public bool PuedeCambiarEstado { get; set; }
        public string? EstadoActual { get; set; }
        public IEnumerable<SelectListItem>? Estados { get; set; }
    }
}
