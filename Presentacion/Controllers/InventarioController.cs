using Microsoft.AspNetCore.Mvc;
using LogicaNegocio;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentacion.Controllers
{
    public class InventarioController : Controller
    {
        private readonly LogicaInventario _logicaInventario = new LogicaInventario();

        [HttpGet]
        public IActionResult Index(int? idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            List<Inventario> historial = new List<Inventario>();

            // Valores por defecto: si no hay fechas, mostramos el último mes
            DateTime inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            DateTime fin = fechaFin ?? DateTime.Now;

            try
            {
                if (idProducto.HasValue && idProducto.Value > 0)
                {
                    // Filtro por producto específico
                    historial = _logicaInventario.ListarHistorialPorProducto(idProducto.Value);
                    ViewBag.IdProducto = idProducto;
                }
                else
                {
                    // Filtro por rango de fechas general
                    historial = _logicaInventario.ListarPorFechas(inicio, fin);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al obtener inventario: " + ex.Message;
            }

            // Mantener los valores en los inputs de la vista
            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");

            return View(historial);
        }
    }
}