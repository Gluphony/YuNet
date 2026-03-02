using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Carrito
    {
        public int ID_CARRITO { get; set; }
        public int ID_USUARIO { get; set; }
        public DateTime FECHA_CREACION { get; set; }
        public decimal TOTAL { get; set; }
        public string? ESTADO { get; set; }
    }
}
