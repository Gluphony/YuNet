using Entidades;
using LogicaNegocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Presentacion.Controllers
{
    public class PedidoController : Controller
    {
        // Instancias de tus capas de Lógica de Negocio
        private readonly LogicaPedido _logicaPedido = new LogicaPedido();
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();
        private readonly LogicaDetallePedido _logicaDetalle = new LogicaDetallePedido();

        // ==========================================
        // GET: Pedido/Index (Tabla general para el Admin)
        // ==========================================
        public ActionResult Index()
        {
            try
            {
                var listaPedidos = _logicaPedido.Listar();
                return View(listaPedidos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la lista de pedidos: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        [HttpGet]
        public ActionResult Historial()
        {
            try
            {
                // 1. Obtenemos el ID del usuario directamente de las Claims de su sesión
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("IdUsuario")?.Value;

                if (string.IsNullOrEmpty(claimId))
                {
                    // Si por alguna razón la sesión se perdió, lo mandamos al Login
                    return RedirectToAction("Login", "Cuenta");
                }

                int idUsuarioActual = int.Parse(claimId);

                // 2. Filtramos los pedidos para que solo vea los suyos
                var misPedidos = _logicaPedido.ListarPorUsuario(idUsuarioActual);

                return View(misPedidos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar tu historial de compras: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        public ActionResult Details(int id)
        {
            try
            {
                // 1. Obtenemos la información general del pedido
                var pedido = _logicaPedido.ObtenerPorId(id);

                // Si alguien intenta buscar un ID que no existe, lo regresamos
                if (pedido == null)
                {
                    TempData["MensajeError"] = "El pedido solicitado no existe o fue eliminado.";
                    return RedirectToAction("Index");
                }

                // 2. Buscamos los productos que pertenecen a este ID de pedido
                var listaProductos = _logicaPedido.ObtenerDetallesPorPedido(id);

                // 3. Guardamos la lista en el ViewBag para leerla en el HTML
                ViewBag.Detalles = listaProductos;

                // 4. Mandamos el pedido a la vista como modelo principal
                return View(pedido);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar los detalles del pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // POST PARA ASIGNAR UN SOLO IMEI (AJAX) - OPCIÓN A
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult AsignarImei(int idDetallePedido, string imei)
        {
            try
            {
                if (idDetallePedido <= 0 || string.IsNullOrWhiteSpace(imei))
                {
                    return Json(new { success = false, message = "El ID del detalle o el IMEI son inválidos." });
                }

                // Llamamos a la lógica enviando el IMEI único
                bool respuesta = _logicaDetalle.AsignarImeiDespacho(idDetallePedido, imei.Trim());

                if (respuesta)
                {
                    return Json(new { success = true, message = "El IMEI fue asignado y descontado del inventario correctamente." });
                }

                return Json(new { success = false, message = "No se pudo despachar el dispositivo por un problema interno." });
            }
            catch (Exception ex)
            {
                // Esto captura los errores de SQL y se los manda al SweetAlert
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        public ActionResult Ticket(int id)
        {
            try
            {
                List<DetallePedido> listaDetalles = _logicaDetalle.ListarTicket(id);

                if (listaDetalles == null || !listaDetalles.Any())
                {
                    ViewBag.Error = "No se encontraron los productos para este pedido o el pedido está vacío.";
                    return View(new List<DetallePedido>());
                }

                ViewBag.NumeroPedido = id;

                return View(listaDetalles);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar el ticket del pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // POST: Pedido/ActualizarEstado
        // ==========================================
        [HttpPost]
        public ActionResult ActualizarEstado(int idPedido, string nuevoEstado)
        {
            try
            {
                // Llamamos a tu lógica de negocio
                bool exito = _logicaPedido.ActualizarEstado(idPedido, nuevoEstado);

                if (exito)
                {
                    // Devolvemos JSON confirmando el éxito
                    return Json(new { success = true });
                }
                else
                {
                    // Devolvemos JSON indicando que falló lógicamente
                    return Json(new { success = false, message = "No se pudo actualizar el estado en la base de datos." });
                }
            }
            catch (Exception ex)
            {
                // Devolvemos JSON con el error exacto
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================
        private void CargarUsuariosDropdown()
        {
            try
            {
                var dtUsuarios = _logicaUsuario.ListarUsuarios();
                var listaOpciones = new List<SelectListItem>();

                if (dtUsuarios != null)
                {
                    foreach (System.Data.DataRow row in dtUsuarios.Rows)
                    {
                        listaOpciones.Add(new SelectListItem
                        {
                            Value = row["ID_USUARIO"].ToString(),
                            Text = $"{row["NOMBRE"]} (ID: {row["ID_USUARIO"]})"
                        });
                    }
                }
                ViewBag.ListaUsuarios = listaOpciones;
            }
            catch (Exception)
            {
                ViewBag.ListaUsuarios = new List<SelectListItem>();
            }
        }
    }
}