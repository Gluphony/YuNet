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

        // --- PROPIEDADES EXTRA PARA EL REPORTE (VISTAS/SP) ---
        public int FACTURA_ORIGINAL { get; set; }
        public string? PRODUCTO_NOMBRE { get; set; }
        public string? IMEI { get; set; }
        public string? AUTORIZADO_POR_NOMBRE { get; set; }
        public string? ESTADO_REPORTE { get; set; }

        // 👇 PROPIEDADES PARA EVITAR EL ERROR EN LA VISTA DEL PDF 👇
        // Aunque SQL no las llene ahorita, C# necesita saber que existen 
        // para poder compilar la vista OrdenServicio.cshtml
        public string? CLIENTE { get; set; }
        public string? CORREO_CLIENTE { get; set; }
        // Propiedad para el dinero de la Nota de Crédito
        public decimal MONTO_REEMBOLSO { get; set; }
        // Propiedad para la imagen de evidencia de "Otro Problema"
        public string? RUTA_EVIDENCIA { get; set; }
    }
}