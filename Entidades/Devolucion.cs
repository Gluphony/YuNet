using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Devolucion
    {
        public int IdDevolucion { get; set; }
        public int IdDetallePedido { get; set; }
        public int IdUsuarioAutoriza { get; set; }
        public DateTime FechaDevolucion { get; set; }
        public string? MotivoCliente { get; set; }
        public string? EstadoDispositivoRecibido { get; set; }
        public string? AccionTomada { get; set; }
        public bool RegresarInventario { get; set; }
    }
}
