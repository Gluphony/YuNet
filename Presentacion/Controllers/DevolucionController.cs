using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Linq; // <-- ¡ESTA ES LA LIBRERÍA QUE FALTABA!

namespace Presentacion.Controllers
{
    [AllowAnonymous]
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
        // ========================================================
        // 3. POST: El cliente envía su reporte
        // ========================================================
        [HttpPost]
        public IActionResult Create([FromBody] DevolucionRequest request) // Usamos una clase pequeña para recibir el JSON
        {
            try
            {
                // Validación de seguridad básica
                if (request == null || request.ID_DETALLE_PEDIDO <= 0)
                {
                    return Json(new { exito = false, mensaje = "Datos inválidos enviados al servidor." });
                }

                // Llamamos a tu lógica
                bool respuesta = _logicaDevolucion.Crear(request.ID_DETALLE_PEDIDO, request.MOTIVO_CLIENTE, out string mensajeRespuesta);

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
                // Obtenemos el ID del Admin que está conectado actualmente.
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;
                if (claimId != null)
                {
                    devolucionAdmin.ID_USUARIO_AUTORIZA = int.Parse(claimId);
                }
                else
                {
                    // Si por alguna razón no hay sesión, le ponemos un usuario por defecto
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
        // 5. GET: Detalles de la Devolución (SOLO LECTURA)
        // ========================================================
        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                // Traemos todas y filtramos por el ID 
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

    // CLASE AUXILIAR: Pon esto al final de tu archivo DevolucionController.cs, 
    // antes de la última llave }
    public class DevolucionRequest
    {
        public int ID_DETALLE_PEDIDO { get; set; }
        public string MOTIVO_CLIENTE { get; set; }
    }
}