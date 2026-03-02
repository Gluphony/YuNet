using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Proveedor
    {
        public int ID_PROVEEDOR { get; set; }
        public string? RAZON_SOCIAL { get; set; }
        public string? NOMBRE_CONTACTO { get; set; }
        public string? TELEFONO { get; set; }
        public string? CORREO { get; set; }
        public string? DIRECCION { get; set; }
        public bool ESTADO { get; set; }
    }
}
