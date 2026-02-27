using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdUsuario { get; set; }
        public int? IdMetodoPago { get; set; } // Puede ser null inicialmente
        public DateTime FechaPedido { get; set; }
        public string? Estado { get; set; }
        public decimal Total
        {
            get; set;
        }
    }
}
