using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("PERMISO",Schema ="Seguridad")]
    public class Permisos
    {
        [Key]
        public int ID_PERMISO { get; set; }
        public string? CLAVE_PERMISO { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool Estado { get; set; }
    }
}
