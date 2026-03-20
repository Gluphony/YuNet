using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Permisos
    {
        public int ID_PERMISO { get; set; }
        public string? CLAVE_PERMISO { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ESTADO { get; set; }
    }
}
