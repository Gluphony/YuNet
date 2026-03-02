using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class DetalleCarrito
    {
        public int ID_DETALLE { get; set; }
        public int ID_CARRITO { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int CANTIDAD { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }

        // Propiedad calculada: No necesita 'set' porque se calcula sola
        public decimal SUBTOTAL => CANTIDAD * PRECIO_UNITARIO;

        // Propiedad de navegación para mostrar el nombre en la tabla
        public string? NOMBRE_PRODUCTO { get; set; }
    }
}
