using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Devolucion
    {

            public int ID_DEVOLUCION { get; set; }
            public int ID_DETALLE_DEVOLUCION { get; set; }
            public int ID_USUARIO_AUTORIZA { get; set; }
            public DateTime FECHA_DEVOLUCION { get; set; }
            public string? MOTIVO_CLIENTE { get; set; }
            public string? ESTADO_DISPOSITIVO_RECIBIDO { get; set; }
            public string? ACCION_TOMADA { get; set; }
            public bool REGRESAR_INVENTARIO { get; set; }

            // --- PROPIEDADES EXTRA PARA EL REPORTE (SP) ---
            public int FACTURA_ORIGINAL { get; set; }
            public string? PRODUCTO_NOMBRE { get; set; }
            public string? IMEI { get; set; }
            public string? AUTORIZADO_POR_NOMBRE { get; set; }
        }
    }