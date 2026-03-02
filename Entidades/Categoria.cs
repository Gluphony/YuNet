using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    [Table("CATEGORIA", Schema = "Catalogo")]
    public class Categoria
    {
        [Key]
        public int ID_CATEGORIA { get; set; }
        public string? NOMBRE_CATEGORIA{ get; set; }
        public bool ESTADO { get; set; }
    }
}
