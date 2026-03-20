using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Auditoria
    {
        public long ID_AUDITORIA { get; set; } // BIGINT mapea a long
        public DateTime FECHA_REGISTRO { get; set; }
        public int ID_USUARIO { get; set; }
        public string? ACCION { get; set; }
        public string? TABLA_AFECTADA { get; set; }
        public string? DETALLE_ACTIVIDAD { get; set; }
        public string? DIRECCION_IP { get; set; }

        public string? NOMBRE_USUARIO { get; set; }
    }
}
