using System;
using System.Linq;
using System.Data; // ¡Agregado para manejar DataTable y DataView correctamente!
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;

namespace Presentacion.Controllers
{
    [Authorize] // Protegemos el carrito, solo usuarios logueados pueden entrar
    public class CarritoController : Controller
    {
        private readonly LogicaCarrito _logicaCarrito = new LogicaCarrito();

        // Constructor limpio
        public CarritoController()
        {
        }

        // --- MÉTODO AUXILIAR: Extraer el ID del usuario de las "Cookies/Claims" de sesión ---
        private int ObtenerIdUsuarioLogueado()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimId, out int idUsuario))
            {
                return idUsuario;
            }
            return 0; // Retorna 0 si falla (la lógica lo rechazará)
        }

        // ==========================================
        // 1. VISTA PRINCIPAL (MI CARRITO / POS ADMIN)
        // ==========================================
        public IActionResult Index()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioLogueado();
                if (idUsuario <= 0) return RedirectToAction("Index", "Login");

                var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                ViewBag.ListaDetalles = logicaDetalle.Listar(miCarrito.ID_CARRITO);

                // LÓGICA EXCLUSIVA PARA EL ADMIN
                if (User.IsInRole("Administrador"))
                {
                    LogicaProducto logicaProd = new LogicaProducto();
                    DataTable todosLosProductos = logicaProd.ListarProductos();

                    // --- SOLUCIÓN ASUNTO 1: Filtrar productos activos ---
                    DataView vistaProductos = new DataView(todosLosProductos);

                    // IMPORTANTE: Revisa en tu Base de Datos cómo se llama la columna de estado.
                    // Si se llama "ACTIVO" o "ESTADO" y es un BIT/INT, sería "ESTADO = 1"
                    vistaProductos.RowFilter = "ESTADO = 1"; // <-- ¡Cambia 'ACTIVO' por el nombre real de tu columna!

                    ViewBag.ProductosDisponibles = vistaProductos.ToTable();
                    // ----------------------------------------------------

                    LogicaUsuario logicaUsu = new LogicaUsuario();
                    DataTable todosLosUsuarios = logicaUsu.ListarUsuarios();

                    DataView vistaUsuarios = new DataView(todosLosUsuarios);
                    vistaUsuarios.RowFilter = "ID_ROL = 2"; // Filtramos solo clientes
                    ViewBag.ClientesDisponibles = vistaUsuarios.ToTable();
                }

                return View(miCarrito);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar la pantalla: " + ex.Message;
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
            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. CAMBIAR ESTADO
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
        // 4. ACTUALIZAR TOTAL (AJAX)
        // ==========================================
        [HttpPost]
        public IActionResult ActualizarTotal(int idCarrito, decimal nuevoTotal)
        {
            try
            {
                _logicaCarrito.ActualizarTotal(idCarrito, nuevoTotal);
                return Json(new { success = true, message = "Total actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // AGREGAR PRODUCTO AL CARRITO (AJAX)
        // ==========================================
        [HttpPost]
        public IActionResult Agregar([FromBody] JsonElement data)
        {
            try
            {
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

        // ==========================================
        // OBTENER RESUMEN PARA OFFCANVAS
        // ==========================================
        [HttpGet]
        public IActionResult ObtenerResumen()
        {
            int idUsuario = ObtenerIdUsuarioLogueado();
            var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

            LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
            var detalles = logicaDetalle.Listar(miCarrito.ID_CARRITO);

            return PartialView("_CarritoResumen", detalles);
        }

        // ==========================================
        // OBTENER INFO BÁSICA (BADGE Y TOTAL)
        // ==========================================
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

        // ==========================================
        // ELIMINAR ITEM (AJAX)
        // ==========================================
        [HttpPost]
        [Route("DetalleCarrito/EliminarSimple/{id}")]
        public IActionResult EliminarSimple(int id)
        {
            try
            {
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                logicaDetalle.Eliminar(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 5. PROCESAR COMPRA (AHORA SÍ, LIMPIO Y POR CAPAS)
        // ==========================================
        [HttpPost]
        public IActionResult ProcesarCompra(int? idMetodoPago)
        {
            int idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario <= 0) return RedirectToAction("Index", "Login");

            var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

            try
            {
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                var detallesCarrito = logicaDetalle.Listar(miCarrito.ID_CARRITO);

                LogicaPedido logicaPedido = new LogicaPedido();
                int nuevoIdPedido = logicaPedido.ProcesarCompra(idUsuario, idMetodoPago, miCarrito.TOTAL, detallesCarrito);

                _logicaCarrito.VaciarCarrito(miCarrito.ID_CARRITO);

                TempData["MensajeExito"] = $"¡Tu pedido #{nuevoIdPedido} ha sido generado exitosamente!";

                return RedirectToAction("Confirmacion", new { idPedido = nuevoIdPedido });
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // MÉTODOS EXCLUSIVOS PARA EL POS (ADMIN)
        // ==========================================

        [HttpPost]
        public IActionResult AgregarAdmin(int idProducto, int cantidad)
        {
            try
            {
                int idAdmin = ObtenerIdUsuarioLogueado();
                var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idAdmin);

                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                logicaDetalle.Agregar(miCarrito.ID_CARRITO, idProducto, cantidad);

                TempData["MensajeExito"] = "Producto agregado al pedido.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al agregar: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int idDetalle)
        {
            try
            {
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                logicaDetalle.Eliminar(idDetalle);
                TempData["MensajeExito"] = "Producto removido del pedido.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al remover: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcesarCompraAdmin(int idClienteReal, int idMetodoPago)
        {
            try
            {
                int idAdmin = ObtenerIdUsuarioLogueado();
                var carritoAdmin = _logicaCarrito.ObtenerOCrearCarrito(idAdmin);

                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                var detallesCarrito = logicaDetalle.Listar(carritoAdmin.ID_CARRITO);

                if (!detallesCarrito.Any())
                {
                    TempData["MensajeError"] = "No puedes generar un pedido vacío.";
                    return RedirectToAction("Index");
                }

                LogicaPedido logicaPedido = new LogicaPedido();
                int nuevoIdPedido = logicaPedido.ProcesarCompra(idClienteReal, idMetodoPago, carritoAdmin.TOTAL, detallesCarrito);

                _logicaCarrito.VaciarCarrito(carritoAdmin.ID_CARRITO);

                // Asunto 2 completamente implementado aquí:
                TempData["MensajeExito"] = $"¡Pedido #{nuevoIdPedido} generado exitosamente!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al procesar el pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // 6. VISTA DE CONFIRMACIÓN DE PEDIDO
        // ==========================================
        [HttpGet]
        public IActionResult Confirmacion(int idPedido)
        {
            ViewBag.IdPedido = idPedido;
            return View();
        }
    }
}