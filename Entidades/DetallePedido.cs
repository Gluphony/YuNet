using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class DetallePedido
    {
        public int IdDetallePedido { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public string? Producto { get; set; } // Nombre del producto para mostrar en el ticket
        public string? ImeiVendido { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada en memoria
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
