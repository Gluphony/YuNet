using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("AUDITORIA", Schema = "Seguridad")]
    public class Auditoria
    {
        [Key]
        public long ID_AUDITORIA { get; set; } // BIGINT mapea a long
        public DateTime FECHA_REGISTRO { get; set; }
        public int ID_USUARIO { get; set; }
        public string? ACCION { get; set; }
        public string? TABLA_AFECTADA { get; set; }
        public string? DETALLE_ACTIVIDAD { get; set; }
        public string? DIRECCION_IP { get; set; }

        [ForeignKey("ID_USUARIO")]
        public virtual Usuario? ObjUsuario{ get; set; }
    }
}
