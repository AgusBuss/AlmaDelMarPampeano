namespace AlmaDelMarPampeano.ViewModels
{
    public class ConteoItem
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Clase { get; set; } = string.Empty; // clase de color para las barras
    }

    public class PanelViewModel
    {
        public int Dias { get; set; }

        public string Periodo => Dias switch
        {
            7 => "los últimos 7 días",
            30 => "los últimos 30 días",
            90 => "los últimos 90 días",
            _ => "todo el historial"
        };

        // Pedidos
        public int TotalPedidos { get; set; }
        public int PedidosCatalogo { get; set; }
        public int PedidosMedida { get; set; }
        public List<ConteoItem> PorEstado { get; set; } = new();

        // Dinero
        public decimal VentasEstimadas { get; set; }
        public decimal Cobrado { get; set; }
        public int PagosPorRevisar { get; set; }

        // Catálogo
        public int LamparasDisponibles { get; set; }
        public int LamparasNoDisponibles { get; set; }
        public List<ConteoItem> ColeccionesTop { get; set; } = new();
        public List<ConteoItem> CampanasTop { get; set; } = new();

        // Actividad del sistema (auditoría)
        public int TotalOperaciones { get; set; }
        public int UsuariosActivos { get; set; }
        public List<ConteoItem> Actividad { get; set; } = new();
        public List<string> UltimasAcciones { get; set; } = new();

        // Informe redactado por la IA (solo cuando se lo pide)
        public string? Reporte { get; set; }
    }
}