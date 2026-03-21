using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Usuario
    {
        public int ID_USUARIO { get; set; }
        public string? NOMBRE { get; set; }
        public string? CORREO { get; set; }
        public byte[]? CONTRASEÑA { get; set; }
        public bool ESTADO { get; set; }
        public int ID_ROL { get; set; }
        public string? NOMBRE_ROL { get; set; }
    }
}
