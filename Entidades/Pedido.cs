using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("PEDIDO", Schema = "Venta")]
    public class Pedido
    {
        [Key]
        public int ID_PEDIDO { get; set; }
        public int ID_USUARIO { get; set; }
        public int? ID_METODO_PAGO { get; set; } 
        public DateTime FECHA_PEDIDO { get; set; }
        public string? ESTADO { get; set; }
        public decimal TOTAL { get; set; }
        [ForeignKey("ID_USUARIO")]
        public virtual Usuario? ObjUsuario { get; set; }
        [ForeignKey("ID_METODO_PAGO")]
        public virtual MetodoPago? ObjMetodoPago { get; set; }
    }
}
