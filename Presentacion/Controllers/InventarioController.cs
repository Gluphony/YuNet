using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LogicaNegocio;
using Entidades;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    // Candado: Solo Administrador y Vendedor pueden ver inventario
    [Authorize(Roles = "Administrador, Admin, Auditor, Vendedor")]
    public class InventarioController : Controller
    {
        private readonly LogicaInventario _logicaInventario = new LogicaInventario();

        [HttpGet]
        public IActionResult Index(int? idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            List<Inventario> historial = new List<Inventario>();

            DateTime inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            DateTime fin = fechaFin ?? DateTime.Now;

            try
            {
                if (idProducto.HasValue && idProducto.Value > 0)
                {
                    historial = _logicaInventario.ListarHistorialPorProducto(idProducto.Value);
                    ViewBag.IdProducto = idProducto;
                }
                else
                {
                    historial = _logicaInventario.ListarPorFechas(inicio, fin);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al obtener inventario: " + ex.Message;
            }

            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");

            return View(historial);
        }
    }
}