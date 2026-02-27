using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Rol
    {
        public  int IdRol { get; set; }
        public string? NombreRol { get; set; }
        public string? Descripcion { get; set; }
        public Boolean Estado { get; set; }
    }
}
