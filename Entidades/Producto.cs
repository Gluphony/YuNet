using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public int IdMarca { get; set; }
        public int? IdCategoria { get; set; } // Nullable según tu script

        public int? IdProveedor { get; set; }
        public string? Modelo { get; set; }
        public string? Descripcion { get; set; }
        public string? Caracteristicas { get; set; }
        public decimal Precio { get; set; }
        public int Existencias { get; set; }
        public byte[]? Imagen { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Estado { get; set; }
    }
}
