using Microsoft.AspNetCore.Mvc;
using LogicaNegocio; // Para acceder a tus productos
using Entidades;
using System.Collections.Generic;
using System.Linq;

namespace Presentacion.Controllers
{
    public class TiendaController : Controller
    {
        // Instanciamos la lógica (Ajusta el nombre si tu clase se llama distinto, ej: LogicaProducto)
        private readonly LogicaInventario _logica = new LogicaInventario();

        public IActionResult Catalogo()
        {
            // 1. Obtenemos los productos de la base de datos
            // Nota: Aquí deberías usar un método que liste productos. 
            // Si no tienes uno en LogicaInventario, usa el de tu clase de productos.
            List<Producto> lista = new List<Producto>();

            try
            {
                // Supongamos que tienes este método en tu lógica
                // lista = _logica.ListarProductosActivos(); 
            }
            catch { /* Manejo de error */ }

            // 2. Pasamos la lista a la vista Catalogo.cshtml
            return View(lista);
        }
    }
}