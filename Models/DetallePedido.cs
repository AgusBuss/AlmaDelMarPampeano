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

        // CORRECCIÓN ACÁ: El int debe llamarse igual que el ForeignKey
        public int MaterialBaseId { get; set; }
        [ForeignKey("MaterialBaseId")]
        public MaterialBase? MaterialBase { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}