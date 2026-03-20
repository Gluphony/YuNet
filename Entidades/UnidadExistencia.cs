using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class UnidadExistencia
    {
        public int ID_UNIDAD { get; set; }
        public int ID_PRODUCTO { get; set; }
        public string? IMEI { get; set; }
        public string? ESTADO_FISICO { get; set; }
        public DateTime FECHA_INGRESO { get; set; }
        public bool VENDIDO { get; set; }
    }
}
