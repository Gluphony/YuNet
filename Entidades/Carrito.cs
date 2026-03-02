using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("CARRITO", Schema = "Venta")]
    public class Carrito
    {
        [Key]
        public int ID_CARRITO { get; set; }
        public int ID_USUARIO { get; set; }
        public DateTime FECHA_CREACION { get; set; }
        public decimal TOTAL { get; set; }
        public string? ESTADO { get; set; }

        [ForeignKey("ID_USUARIO")]
        public virtual Usuario? ObjUsuario { get; set; }
    }
}
