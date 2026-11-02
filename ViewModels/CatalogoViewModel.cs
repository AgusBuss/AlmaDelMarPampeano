using AlmaDelMarPampeano.Models;

namespace AlmaDelMarPampeano.ViewModels
{
    public class CatalogoViewModel
    {
        public List<Coleccion> Colecciones { get; set; } = new();
        public int? ColeccionId { get; set; }
        public List<Lampara> Lamparas { get; set; } = new();
    }
}