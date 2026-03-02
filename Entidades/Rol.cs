using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("ROL",Schema = "Seguridad")]
    public class Rol
    {
        [Key]
        public  int ID_ROL { get; set; }
        public string? NOMBRE_ROL { get; set; }
        public string? DESCRIPCION { get; set; }
        public bool ESTADO { get; set; }
    }
}
