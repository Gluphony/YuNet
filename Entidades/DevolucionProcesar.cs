using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class DevolucionProcesar
    {
        public int ID_DEVOLUCION { get; set; }
        public string? ESTADO_REPORTE { get; set; }
        public string? ESTADO_DISPOSITIVO_RECIBIDO { get; set; }
        public string? ACCION_TOMADA { get; set; }
        public bool REGRESAR_INVENTARIO { get; set; }

    }
}
