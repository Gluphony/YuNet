using System;

namespace Entidades
{
    public class Inventario
    {
        public int ID_INVENTARIO { get; set; }
        public int ID_PRODUCTO { get; set; }

        // ¡NUEVA PROPIEDAD PARA LA VISTA!
        public string? MODELO { get; set; }

        public int ID_USUARIO { get; set; }
        public string? TIPO_MOVIMIENTO { get; set; }
        public int CANTIDAD { get; set; }
        public string? MOTIVO { get; set; }
        public DateTime FECHA_MOVIMIENTO { get; set; }
    }
}