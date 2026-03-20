using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class RolPermiso
    {
        public int ID_ROL { get; set; }
        public int ID_PERMISO { get; set; }
        public string? NOMBRE_ROL { get; set; }
        public string? NOMBRE_PERMISO { get; set; }
    }
}
