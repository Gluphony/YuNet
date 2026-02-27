using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public byte[]? Contrasena { get; set; } // VARBINARY mapea a byte[]
        public bool Estado { get; set; }
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
    }
}
