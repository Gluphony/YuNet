using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class UnidadExistencia
    {
        public int IdUnidad { get; set; }
        public int IdProducto { get; set; }
        public string? Imei { get; set; }
        public string? EstadoFisico { get; set; }
        public DateTime FechaIngreso { get; set; }
        public bool Vendido { get; set; }
    }
}
