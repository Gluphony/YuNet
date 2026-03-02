using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("MARCA", Schema = "Catalogo")]
    public class Marca
    {
        [Key]
        public int ID_MARCA { get; set; }
        public string? NOMBRE { get; set; }
        public bool Estado { get; set; }
    }
}
