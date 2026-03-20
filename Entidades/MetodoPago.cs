using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class MetodoPago
    {
        public int ID_METODO_PAGO { get; set; }
        public string? NOMBRE_METODO { get; set; }
        public bool ESTADO { get; set; }
    }
}
