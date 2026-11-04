using AlmaDelMarPampeano.Models;

namespace AlmaDelMarPampeano.ViewModels
{
    // Datos de la lista de pedidos (cliente: los suyos; administrador: todos)
    public class PedidosIndexViewModel
    {
        public List<Pedido> Pedidos { get; set; } = new();
        public bool EsAdmin { get; set; }

        // Solo se completan para el administrador
        public int? EstadoId { get; set; }
        public List<EstadoPedido> Estados { get; set; } = new();
        public Dictionary<string, string> Emails { get; set; } = new();
    }
}