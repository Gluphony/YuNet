using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Presentacion.Controllers
{
    // Bloqueamos todo el controlador por defecto solo para estos roles
    [Authorize(Roles = "Administrador,Vendedor")]
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
        // 2. POST: El cliente envía su reporte (Accesible para todos)
        // ========================================================
        [HttpPost]
        [AllowAnonymous] // <-- Solo este método es público para que el cliente reporte
        public IActionResult Create(int ID_DETALLE_PEDIDO, string MOTIVO_CLIENTE)
        {
            try
            {
                if (ID_DETALLE_PEDIDO <= 0)
                {
                    return Json(new { exito = false, mensaje = "ID de producto inválido." });
                }

                bool respuesta = _logicaDevolucion.Crear(ID_DETALLE_PEDIDO, MOTIVO_CLIENTE, out string mensajeRespuesta);

                return Json(new { exito = respuesta, mensaje = mensajeRespuesta });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error inesperado: " + ex.Message });
            }
        }

        // ========================================================
        // 3. POST: El Admin aprueba o rechaza 
        // ========================================================
        [HttpPost]
        public IActionResult Procesar(Devolucion devolucionAdmin)
        {
            try
            {
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;
                if (claimId != null)
                {
                    devolucionAdmin.ID_USUARIO_AUTORIZA = int.Parse(claimId);
                }
                else
                {
                    devolucionAdmin.ID_USUARIO_AUTORIZA = 1;
                }

                bool respuesta = _logicaDevolucion.ProcesarAdmin(devolucionAdmin, out string mensajeRespuesta);

                if (respuesta)
                {
                    TempData["MensajeExito"] = mensajeRespuesta;
                }
                else
                {
                    TempData["MensajeError"] = mensajeRespuesta;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error crítico al procesar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ========================================================
        // 4. GET: Detalles de la Devolución (SOLO LECTURA)
        // ========================================================
        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                var devolucion = _logicaDevolucion.Listar().FirstOrDefault(d => d.ID_DEVOLUCION == id);

                if (devolucion == null)
                {
                    TempData["MensajeError"] = "No se encontró la devolución solicitada.";
                    return RedirectToAction("Index");
                }

                return View(devolucion);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar los detalles: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}