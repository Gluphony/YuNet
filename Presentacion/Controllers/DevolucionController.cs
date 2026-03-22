using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Security.Claims; // Necesario para obtener el ID de usuario de la sesión

namespace Presentacion.Controllers
{
    public class DevolucionController : Controller
    {
        private readonly LogicaDevolucion _logicaDevolucion = new LogicaDevolucion();

        // --- 1. ACCIÓN PARA MOSTRAR LA LISTA DE DEVOLUCIONES ---
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
        // NUEVO: GET: Devolucion/Create (Muestra el formulario)
        // Recibe el ID del detalle del pedido a devolver
        // ========================================================
        // ========================================================
        // GET: Devolucion/Create
        // ========================================================
        [HttpGet]
        public IActionResult Create(int? idDetalle)
        {
            var modelo = new Devolucion();

            if (idDetalle.HasValue)
            {
                // Ahora sí usamos la propiedad exacta de tu Entidad
                modelo.ID_DETALLE_DEVOLUCION = idDetalle.Value;
                modelo.FECHA_DEVOLUCION = DateTime.Now;
                modelo.REGRESAR_INVENTARIO = false; // Valor por defecto
            }
            else
            {
                ViewBag.Error = "Atención: No se ha seleccionado un producto específico para devolver.";
            }

            return View(modelo);
        }

        // ========================================================
        // POST: Devolucion/Create
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Devolucion nuevaDevolucion)
        {
            try
            {
                // 1. Asignar el usuario que autoriza (desde Claim o estático)
                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                nuevaDevolucion.ID_USUARIO_AUTORIZA = !string.IsNullOrEmpty(claimId) ? int.Parse(claimId) : 1;
                nuevaDevolucion.FECHA_DEVOLUCION = DateTime.Now;

                // 2. Limpiamos validaciones de PK, FKs y propiedades de solo lectura/reporte
                ModelState.Remove("ID_DEVOLUCION");
                ModelState.Remove("ID_USUARIO_AUTORIZA");
                ModelState.Remove("PRODUCTO_NOMBRE");
                ModelState.Remove("IMEI");
                ModelState.Remove("AUTORIZADO_POR_NOMBRE");
                // FACTURA_ORIGINAL al ser int, si no viene en el form, asume 0. Lo removemos por si acaso.
                ModelState.Remove("FACTURA_ORIGINAL");

                if (ModelState.IsValid)
                {
                    _logicaDevolucion.Crear(nuevaDevolucion);

                    TempData["MensajeExito"] = "¡La devolución se registró correctamente!";
                    return RedirectToAction("Index", "Pedido"); // Te regreso a pedidos tras el éxito
                }

                ViewBag.Error = "Por favor, revisa los campos del formulario.";
                return View(nuevaDevolucion);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar la devolución: " + ex.Message;
                return View(nuevaDevolucion);
            }
        }
    }
}