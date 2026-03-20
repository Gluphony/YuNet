using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class DetallePedido
    {
        public int ID_DETALLE_PEDIDO { get; set; }
        public int ID_PEDIDO { get; set; }
        public int ID_PRODUCTO { get; set; }
        public string? PRODUCTO { get; set; } // Nombre del producto para mostrar en el ticket
        public string? IMEI_VENDIDO { get; set; }
        public int CANTIDAD { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }

        // Propiedad calculada en memoria
        public decimal SUBTOTAL => CANTIDAD * PRECIO_UNITARIO;
    }
}
