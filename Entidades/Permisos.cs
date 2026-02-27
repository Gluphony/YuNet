using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Permisos
    {
        public int IdPermiso { get; set; }
        public string? ClavePermiso { get; set; }
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}
