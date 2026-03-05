using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Security.Claims;

namespace Presentacion.Controllers
{
    [Authorize]
    public class DetalleCarritoController : Controller
    {
        // Instanciamos tu lógica exacta
        private readonly LogicaDetalleCarrito _logicaDetalle = new LogicaDetalleCarrito();

        // Necesitamos la cabecera solo para obtener el ID_CARRITO del usuario logueado
        private readonly LogicaCarrito _logicaCarrito = new LogicaCarrito();

        // --- MÉTODO AUXILIAR PARA OBTENER EL USUARIO ---
        private int ObtenerIdUsuarioLogueado()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimId, out int idUsuario))
            {
                return idUsuario;
            }
            return 0;
        }

        // ==========================================
        // 1. AGREGAR PRODUCTO AL CARRITO
        // ==========================================
        [HttpPost]
        public IActionResult Agregar(int idProducto, int cantidad = 1)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioLogueado();
                if (idUsuario <= 0) return RedirectToAction("Index", "Login");

                // 1. Obtenemos el ID del carrito del usuario (o lo creamos si no tiene)
                var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

                // 2. Ejecutamos TU método exacto de la lógica
                _logicaDetalle.Agregar(miCarrito.ID_CARRITO, idProducto, cantidad);

                TempData["MensajeExito"] = "Producto agregado al carrito correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al agregar el producto: " + ex.Message;
            }

            // Lo enviamos a la vista principal del carrito
            return RedirectToAction("Index", "Carrito");
        }

        // ==========================================
        // 2. ELIMINAR ÍTEM DEL CARRITO
        // ==========================================
        [HttpPost]
        public IActionResult Eliminar(int idDetalle)
        {
            try
            {
                // Ejecutamos TU método exacto de la lógica
                _logicaDetalle.Eliminar(idDetalle);
                TempData["MensajeExito"] = "Producto eliminado del carrito.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar: " + ex.Message;
            }

            // Regresamos a la vista principal del carrito
            return RedirectToAction("Index", "Carrito");
        }
    }
}