using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class DevolucionController : Controller
    {
        private readonly LogicaDevolucion _logicaDevolucion = new LogicaDevolucion();

        // ========================================================
        // 1. GET: MUESTRA LA LISTA DE DEVOLUCIONES (Para el Admin)
        // ========================================================
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                List<Devolucion> devoluciones = _logicaDevolucion.Listar();
                return View(devoluciones);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Devolucion>());
            }
        }

        // ========================================================
        // 2. GET: Muestra el formulario para crear devolución
        // ========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Devolucion());
        }

        // ========================================================
        // 3. POST: El cliente envía su reporte
        // ========================================================
        [HttpPost]
        public IActionResult Create(int ID_DETALLE_PEDIDO, string MOTIVO_CLIENTE)
        {
            try
            {
                // Llamamos a tu lógica
                bool respuesta = _logicaDevolucion.Crear(ID_DETALLE_PEDIDO, MOTIVO_CLIENTE, out string mensajeRespuesta);

                if (respuesta)
                {
                    // Devolvemos JSON para que Javascript lo atrape sin recargar la página
                    return Json(new { exito = true, mensaje = mensajeRespuesta });
                }
                else
                {
                    return Json(new { exito = false, mensaje = mensajeRespuesta });
                }
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error inesperado: " + ex.Message });
            }
        }

        // ========================================================
        // 4. POST: El Admin aprueba o rechaza (NUEVO)
        // ========================================================
        [HttpPost]
        public IActionResult Procesar(Devolucion devolucionAdmin)
        {
            try
            {
                bool respuesta = _logicaDevolucion.ProcesarAdmin(devolucionAdmin, out string mensajeRespuesta);

                if (respuesta)
                {
                    TempData["MensajeExito"] = mensajeRespuesta;
                }
                else
                {
                    TempData["MensajeError"] = mensajeRespuesta;
                }

                // Recargamos la tabla del administrador
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error crítico al procesar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}