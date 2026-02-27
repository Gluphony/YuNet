using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class DetalleCarrito
    {
        public int IdDetalle { get; set; }
        public int IdCarrito { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada: No necesita 'set' porque se calcula sola
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // Propiedad de navegación para mostrar el nombre en la tabla
        public string? NombreProducto { get; set; }
    }
}
