using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class DetalleCompra
    {
        public int ID_DETALLE_COMPRA { get; set; }
        public int ID_COMPRA { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int CANTIDAD { get; set; }
        public decimal COSTO_UNITARIO { get; set; }
        public decimal SUBTOTAL { get; set; }
        // Esta propiedad es temporal para el transporte de los IMEIs al SQL
        public string? LISTA_IMEIS { get; set; }
    }
}