using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlmaDelMarPampeano.Models
{
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        public int MaterialCampanaId { get; set; }
        [ForeignKey("MaterialCampanaId")]
        public MaterialCampana? MaterialCampana { get; set; }

        public int MaterialBaseId { get; set; }
        [ForeignKey("MaterialBaseId")]
        public MaterialBase? MaterialBase { get; set; }

        // Tipo de luz elegido (fría, cálida o multicolor).
        // Es opcional en la base de datos para no romper las lámparas cargadas antes de existir este campo;
        // el formulario de alta lo exige.
        public int? TipoLuzId { get; set; }
        [ForeignKey("TipoLuzId")]
        public TipoLuz? TipoLuz { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}