using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Rol
    {
        public  int ID_ROL { get; set; }
        public string? NOMBRE_ROL { get; set; }
        public string? DESCRIPCION  { get; set; }
        public Boolean ESTADO { get; set; }
    }
}
