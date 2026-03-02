using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("PROVEEDOR", Schema = "Inventario")]
    public class Proveedor
    {
        [Key]
        public int ID_PROVEEDOR { get; set; }
        public string? RAZON_SOCIAL { get; set; }
        public string? NOMBRE_CONTACTO { get; set; }
        public string? TELEFONO { get; set; }
        public string? CORREO { get; set; }
        public string? DIRECCION { get; set; }
        public bool ESTADO { get; set; }
    }
}
