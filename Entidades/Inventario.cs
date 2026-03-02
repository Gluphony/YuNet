using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Inventario
    {
        public int ID_INVENTARIO { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int ID_USUARIO { get; set; }
        public string? TIPO_MOVIMIENTO { get; set; }
        public int CANTIDAD { get; set; }
        public string? MOTIVO { get; set; }
        public DateTime FECHA_MOVIMIENTO { get; set; }
    }
}
