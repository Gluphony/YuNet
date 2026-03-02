using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("INVENTARIO", Schema = "Inventario")]
    public class Inventario
    {
        [Key]
        public int ID_INVENTARIO { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int ID_USUARIO { get; set; }
        public string? TIPO_MOVIMIENTO { get; set; }
        public int CANTIDAD { get; set; }
        public string? MOTIVO { get; set; }
        public DateTime FECHA_MOVIMIENTO { get; set; }
        [ForeignKey("ID_PRODUCTO")]
        public virtual Producto? ObjProducto { get; set; }

        [ForeignKey("ID_USUARIO")]
        public virtual Usuario? ObjUsuario { get; set; }
    }
}
