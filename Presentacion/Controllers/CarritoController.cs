using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Nuevo: Para leer la configuración (cadena de conexión)
using Microsoft.Data.SqlClient; // Nuevo: Para ejecutar los SPs con ADO.NET (usa System.Data.SqlClient si tu versión es antigua)
using Entidades;
using LogicaNegocio;

namespace Presentacion.Controllers
{
    [Authorize] // Protegemos el carrito, solo usuarios logueados pueden entrar
    public class CarritoController : Controller
    {
        private readonly LogicaCarrito _logicaCarrito = new LogicaCarrito();
        private readonly string _cadenaConexion;

        // Constructor modificado para inyectar IConfiguration y obtener la cadena de conexión
        public CarritoController(IConfiguration config)
        {
            // OJO: Asegúrate de que "DefaultConnection" sea el nombre exacto de tu conexión en appsettings.json
            _cadenaConexion = config.GetConnectionString("CadenaSQL");
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
        // 1. VISTA PRINCIPAL (MI CARRITO)
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
        // 5. PROCESAR COMPRA (NUEVO CHECKOUT)
        // ==========================================
        [HttpPost]
        public IActionResult ProcesarCompra(int? idMetodoPago)
        {
            int idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario <= 0) return RedirectToAction("Index", "Login");

            int nuevoIdPedido = 0;
            var miCarrito = _logicaCarrito.ObtenerOCrearCarrito(idUsuario);

            try
            {
                LogicaDetalleCarrito logicaDetalle = new LogicaDetalleCarrito();
                var detallesCarrito = logicaDetalle.Listar(miCarrito.ID_CARRITO);

                if (detallesCarrito == null || !detallesCarrito.Any())
                {
                    TempData["MensajeError"] = "Tu carrito está vacío. Agrega productos antes de pagar.";
                    return RedirectToAction("Index");
                }

                using (SqlConnection con = new SqlConnection(_cadenaConexion))
                {
                    con.Open();
                    // Usamos transacciones para que, si falta stock de un producto, todo se cancele limpiamente
                    using (SqlTransaction transaccion = con.BeginTransaction())
                    {
                        try
                        {
                            // A. Crear el Encabezado del Pedido
                            using (SqlCommand cmd = new SqlCommand("Venta.Sp_CrearPedido", con, transaccion))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);
                                cmd.Parameters.AddWithValue("@Id_Metodo_Pago", (object)idMetodoPago ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Total", miCarrito.TOTAL);
                                cmd.Parameters.AddWithValue("@Estado", "PENDIENTE"); // Estado inicial

                                // Ejecutamos y capturamos el ID del nuevo pedido
                                nuevoIdPedido = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // B. Insertar los Detalles (Ciclo por cada producto)
                            foreach (var item in detallesCarrito)
                            {
                                using (SqlCommand cmdDetalle = new SqlCommand("Venta.Sp_InsertarDetallePedido", con, transaccion))
                                {
                                    cmdDetalle.CommandType = CommandType.StoredProcedure;
                                    cmdDetalle.Parameters.AddWithValue("@Id_Pedido", nuevoIdPedido);
                                    cmdDetalle.Parameters.AddWithValue("@Id_Producto", item.ID_PRODUCTO);
                                    cmdDetalle.Parameters.AddWithValue("@Imei_Vendido", DBNull.Value); // Se asignará en almacén
                                    cmdDetalle.Parameters.AddWithValue("@Cantidad", item.CANTIDAD);
                                    cmdDetalle.Parameters.AddWithValue("@Precio_Unitario", item.PRECIO_UNITARIO);
                                    cmdDetalle.Parameters.AddWithValue("@Id_Usuario", idUsuario); // Para Kárdex

                                    cmdDetalle.ExecuteNonQuery();
                                }
                            }

                            // C. Confirmar la transacción si todo fue un éxito
                            transaccion.Commit();
                        }
                        catch (SqlException sqlEx)
                        {
                            // Atrapa los RAISERROR del SP (ej. Stock insuficiente)
                            transaccion.Rollback();
                            TempData["MensajeError"] = "Error al procesar el pedido: " + sqlEx.Message;
                            return RedirectToAction("Index");
                        }
                        catch (Exception)
                        {
                            transaccion.Rollback();
                            TempData["MensajeError"] = "Ocurrió un error inesperado al guardar el pedido.";
                            return RedirectToAction("Index");
                        }
                    }
                }

                // Si llegamos hasta aquí, el pago/pedido fue un éxito. Vaciamos el carrito.
                _logicaCarrito.VaciarCarrito(miCarrito.ID_CARRITO);

                TempData["MensajeExito"] = $"¡Tu pedido #{nuevoIdPedido} ha sido generado exitosamente!";

                // Redirigimos a la pantalla de éxito
                return RedirectToAction("Confirmacion", new { idPedido = nuevoIdPedido });
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error crítico al procesar la compra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // 6. VISTA DE CONFIRMACIÓN DE PEDIDO
        // ==========================================
        [HttpGet]
        public IActionResult Confirmacion(int idPedido)
        {
            // Le pasamos el ID a la vista para que pueda mostrar el número de orden
            ViewBag.IdPedido = idPedido;
            return View();
        }
    }
}