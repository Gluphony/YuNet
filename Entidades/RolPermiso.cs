using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("ROL_PERMISO", Schema = "Seguridad")]
    public class RolPermiso
    {
        public int ID_ROL { get; set; }
        public int ID_PERMISO { get; set; }
        [ForeignKey("ID_ROL")]
        public virtual Rol? ObjRol { get; set; }

        [ForeignKey("ID_PERMISO")]
        public virtual Permisos? ObjPermiso { get; set; }
    }
}
