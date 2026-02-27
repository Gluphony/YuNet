using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Auditoria
    {
        public long IdAuditoria { get; set; } // BIGINT mapea a long
        public DateTime FechaRegistro { get; set; }
        public int IdUsuario { get; set; }
        public string? Accion { get; set; }
        public string? TablaAfectada { get; set; }
        public string? DetalleActividad { get; set; }
        public string? DireccionIp { get; set; }

        public string? Nombre_Usuario { get; set; }
    }
}
