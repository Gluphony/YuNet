using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LogicaNegocio;
using Entidades;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO: Todo el Staff Operativo y el Auditor pueden VER el historial. 
    // Los "Usuarios" (clientes) quedan fuera.
    [Authorize(Roles = "Administrador,Admin,Auditor,Vendedor")]
    public class InventarioController : Controller
    {
        private readonly LogicaInventario _logicaInventario = new LogicaInventario();

        // ========================================================
        // 1. GET: VER HISTORIAL DE MOVIMIENTOS (Entradas/Salidas)
        // ========================================================
        [HttpGet]
        public IActionResult Index(int? idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            List<Inventario> historial = new List<Inventario>();

            // Si no mandan fechas, por defecto mostramos los últimos 30 días
            DateTime inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            DateTime fin = fechaFin ?? DateTime.Now;

            try
            {
                if (idProducto.HasValue && idProducto.Value > 0)
                {
                    // Filtrar por un producto en específico
                    historial = _logicaInventario.ListarHistorialPorProducto(idProducto.Value);
                    ViewBag.IdProducto = idProducto;
                }
                else
                {
                    // Filtrar por rango de fechas (general)
                    historial = _logicaInventario.ListarPorFechas(inicio, fin);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al obtener el historial de inventario: " + ex.Message;
            }

            // Mantenemos las fechas en la vista para que el usuario sepa qué está viendo
            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");

            return View(historial);
        }
    }
}