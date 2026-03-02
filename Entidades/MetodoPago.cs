using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("METODO_PAGO",Schema ="Catalogo")]
    public class MetodoPago
    {
        [Key]
        public int ID_METODO_PAGO { get; set; }
        public string? NOMBRE_METODO { get; set; }
        public bool ESTADO { get; set; }
    }
}
