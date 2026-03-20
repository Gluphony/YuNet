using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Producto
    {
        public int ID_PRODUCTO { get; set; }
        public int ID_MARCA { get; set; }
        public int? ID_CATEGORIA { get; set; } // Nullable según el script

        public int? ID_PROVEEDOR { get; set; }
        public string? MODELO { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? CARACTERISTICAS { get; set; }
        public decimal PRECIO { get; set; }
        public int EXISTENCIAS { get; set; }
        public byte[]? IMAGEN { get; set; }
        public DateTime FECHA_CREACION { get; set; }
        public bool ESTADO { get; set; }
    }
}
