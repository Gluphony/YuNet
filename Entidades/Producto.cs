using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("PRODUCTO", Schema = "Inventario")]
    public class Producto
    {
        [Key]
        public int ID_PRODUCTO { get; set; }
        public int ID_MARCA { get; set; }
        public int? ID_CATEGORIA { get; set; } // Nullable según tu script
        public int ID_PROVEEDOR { get; set; }
        public string? MODELO { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? CARACTERISTICAS { get; set; }
        public decimal PRECIO { get; set; }
        public int EXISTENCIAS { get; set; }
        public string? IMAGEN { get; set; }
        public DateTime FECHA_CREACION { get; set; }
        public bool ESTADO { get; set; }

        [ForeignKey("ID_MARCA")]
        public virtual Marca? ObjMarca { get; set; }

        [ForeignKey("ID_CATEGORIA")]
        public virtual Categoria? ObjCategoria { get; set; }

        [ForeignKey("ID_PROVEEDOR")]
        public virtual Proveedor? ObjProveedor { get; set; }
    }
}
