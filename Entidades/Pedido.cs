using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Pedido
    {
        public int ID_PEDIDO { get; set; }
        public int ID_USUARIO { get; set; }
        public int? ID_METODO_PAGO { get; set; }
        public DateTime FECHA_PEDIDO { get; set; }
        public string? ESTADO { get; set; }
        public decimal TOTAL { get; set; }

        // --- PROPIEDADES PARA MOSTRAR TEXTOS EN LA VISTA ---
        public string? CLIENTE { get; set; }

        public string? METODO_PAGO_NOMBRE { get; set; }

        // --- NUEVA PROPIEDAD: LISTA DE PRODUCTOS DEL PEDIDO ---
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}