using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("DEVOLUCION", Schema = "Venta")]
    public class Devolucion
    {
        [Key]
        public int ID_DEVOLUCION { get; set; }
        public int ID_DETALLE_PEDIDO { get; set; }
        public int ID_USUARIO_AUTORIZA { get; set; }
        public DateTime FECHA_DEVOLUCION { get; set; }
        public string? MOTIVO_CLIENTE { get; set; }
        public string? ESTADO_DISPOSITIVO_RECIBIDO { get; set; }
        public string? ACCION_TOMADA { get; set; }
        public bool REGRESAR_INVENTARIO { get; set; }
        [ForeignKey("ID_DETALLE_PEDIDO")]
        public virtual DetallePedido? ObjDetallePedido { get; set; }
        [ForeignKey("ID_USUARIO_AUTORIZA")]
        public virtual Usuario? ObjUsuarioAutoriza { get; set; }
    }
}
