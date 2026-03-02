using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("USUARIO", Schema = "Seguridad")]
    public class Usuario
    {
        [Key]
        public int ID_USUARIO { get; set; }
        public string? NOMBRE { get; set; }
        public string? CORREO { get; set; }
        public byte[]? CONTRASEÑA { get; set; } // VARBINARY mapea a byte[]
        public bool ESTADO { get; set; }
        public int ID_ROL { get; set; }
        [ForeignKey("ID_ROL")]
        public virtual Rol?     ObjRol{ get; set; }
    }
}
