using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("DETALLE_PEDIDO", Schema = "Venta")]
    public class DetallePedido
    {
        [Key]
        public int ID_DETALLE_PEDIDO { get; set; }
        public int ID_PEDIDO { get; set; }
        public int ID_PRODUCTO { get; set; }
        public string? IMEI_VENDIDO { get; set; }
        public int CANTIDAD { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }

        public decimal SUBTOTAL => CANTIDAD * PRECIO_UNITARIO;

        [ForeignKey("ID_PEDIDO")]
        public virtual Pedido? ObjPedido { get; set; }

        [ForeignKey("ID_PRODUCTO")]
        public virtual Producto? ObjProducto { get; set; }
    }
}
