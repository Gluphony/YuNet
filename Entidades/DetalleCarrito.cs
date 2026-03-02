using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("DETALLE_CARRITO", Schema = "Venta")]
    public class DetalleCarrito
    {
        [Key]
        public int ID_DETALLE { get; set; }
        public int ID_CARRITO { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int CANTIDAD { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal SUBTOTAL { get; private set; }
        [ForeignKey("ID_CARRITO")]
        public virtual Carrito? ObjCarrito { get; set; }
        [ForeignKey("ID_PRODUCTO")]
        public virtual Producto? ObjProducto { get; set; }
    }
}
