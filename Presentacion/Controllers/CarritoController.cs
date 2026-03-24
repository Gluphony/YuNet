using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Security.Claims; // Importante para leer los datos del usuario logueado

namespace Presentacion.Controllers
{
    [Authorize] // Protegemos el carrito, solo usuarios logueados pueden entrar
    public class CarritoController : Controller
    {
        private readonly LogicaCarrito _logicaCarrito = new LogicaCarrito();

        // --- MÉTODO AUXILIAR: Extraer el ID del usuario de las "Cookies/Claims" de sesión ---
        private int ObtenerIdUsuarioLogueado()
        {
            // Busca el ClaimTypes.NameIdentifier (o el nombre que le hayas dado al guardar el ID en tu Login)
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claimId, out int idUsuario))
            {
                return idUsuario;
            }
            return 0; // Retorna 0 si falla (la lógica lo rechazará)
        }

        // ==========================================
        // 1. VISTA PRINCIPAL (MI CARRITO)
        // ==========================================
        public IActionResult Index()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioLogueado();

                // Si por alguna razón expiró la sesión o no hay ID válido, pa' fuera
                if (idUsuario <= 0)
                    return RedirectToAction("Index", "Login");

                // Obtenemos su carrito pendiente o le creamos uno nuevo mágicamente
                var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                ViewBag.ListaDetalles = logicaDetalle.Listar(miCarrito.ID_CARRITO);

                return View(miCarrito);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar tu carrito: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // ==========================================
        // 2. VACIAR CARRITO
        // ==========================================
        [HttpPost]
        public IActionResult Vaciar(int idCarrito)
        {
            try
            {
                _logicaCarrito.VaciarCarrito(idCarrito);
                TempData["MensajeExito"] = "El carrito se ha vaciado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "No se pudo vaciar el carrito: " + ex.Message;
            }

            return RedirectToAction("Index"); // Recargamos la página del carrito
        }

        // ==========================================
        // 3. CAMBIAR ESTADO (Ej. Pagar o Cancelar)
        // ==========================================
        [HttpPost]
        public IActionResult CambiarEstado(int idCarrito, string nuevoEstado)
        {
            try
            {
                _logicaCarrito.CambiarEstado(idCarrito, nuevoEstado);

                if (nuevoEstado.ToUpper() == "COMPLETADO")
                    TempData["MensajeExito"] = "¡Tu compra ha sido procesada con éxito!";
                else if (nuevoEstado.ToUpper() == "CANCELADO")
                    TempData["MensajeExito"] = "El carrito ha sido cancelado.";
                else
                    TempData["MensajeExito"] = $"El estado del carrito cambió a {nuevoEstado}.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // 4. ACTUALIZAR TOTAL (Ideal para peticiones AJAX)
        // ==========================================
        [HttpPost]
        public IActionResult ActualizarTotal(int idCarrito, decimal nuevoTotal)
        {
            try
            {
                _logicaCarrito.ActualizarTotal(idCarrito, nuevoTotal);

                // Devolvemos un JSON en lugar de recargar toda la página.
                // Así cuando sumes un producto, el total se actualiza en vivo.
                return Json(new { success = true, message = "Total actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Agregar([FromBody] JsonElement data)
        {
            try
            {
                // Esto arregla el error rosado
                int idProducto = data.GetProperty("id").GetInt32();

                int idUsuario = ObtenerIdUsuarioLogueado();
                var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                logicaDetalle.Agregar(miCarrito.ID_CARRITO, idProducto, 1);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Este método devuelve el HTML parcial de los items para el Offcanvas
        [HttpGet]
        public IActionResult ObtenerResumen()
        {
            int idUsuario = ObtenerIdUsuarioLogueado();
            var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

            LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
            var detalles = logicaDetalle.Listar(miCarrito.ID_CARRITO);

            // Retorna la vista parcial que creamos antes
            return PartialView("_CarritoResumen", detalles);
        }

        // Este método devuelve el numerito del Badge y el Total para el Navbar
        [HttpGet]
        public JsonResult ObtenerInfoBasica()
        {
            int idUsuario = ObtenerIdUsuarioLogueado();
            var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

            LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
            var detalles = logicaDetalle.Listar(miCarrito.ID_CARRITO);

            int cantidadTotal = 0;
            if (detalles != null)
            {
                foreach (var d in detalles) cantidadTotal += d.CANTIDAD;
            }

            return Json(new
            {
                count = cantidadTotal,
                total = miCarrito.TOTAL
            });
        }
        [HttpPost]
        [Route("DetalleCarrito/EliminarSimple/{id}")]
        public IActionResult EliminarSimple(int id)
        {
            try
            {
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                logicaDetalle.Eliminar(id);

                // (Opcional) Si tu BD no recalcula el total automáticamente, 
                // deberías llamar aquí a tu método para actualizar el total del carrito.

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}