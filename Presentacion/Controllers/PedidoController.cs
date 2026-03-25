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

        // ==========================================
        // GET: Pedido/Historial (Exclusivo para el Cliente)
        // ==========================================
        public ActionResult Historial()
        {
            try
            {
                // TODO: Cambiar este ID "1" por la variable de sesión o Claim de tu usuario logueado.
                // Ejemplo Session: int idUsuarioActual = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
                int idUsuarioActual = 1;

                var misPedidos = _logicaPedido.Listar()
                                              .Where(p => p.ID_USUARIO == idUsuarioActual)
                                              .ToList();

                return View(misPedidos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar tu historial de compras: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        // ==========================================
        // GET: Pedido/Details/5 (Detalle general del pedido)
        // ==========================================
        public ActionResult Details(int id)
        {
            try
            {
                var pedido = _logicaPedido.ObtenerPorId(id);

                if (pedido == null)
                {
                    TempData["MensajeError"] = "El pedido solicitado no existe o fue eliminado.";
                    return RedirectToAction("Index");
                }

                return View(pedido);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar los detalles del pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // GET: Pedido/Ticket/5 (La vista de factura)
        // ==========================================
        // ==========================================
        // GET: Pedido/Ticket/5 (La vista de factura)
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

                // NUEVO: Le pasamos el ID a la vista mediante ViewBag
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
        // GET: Pedido/Create
        // ==========================================
        public ActionResult Create()
        {
            CargarUsuariosDropdown();
            return View(new Pedido());
        }

        // ==========================================
        // POST: Pedido/Create
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pedido nuevoPedido)
        {
            try
            {
                nuevoPedido.FECHA_PEDIDO = DateTime.Now;

                // Limpiamos validaciones
                ModelState.Remove("ID_PEDIDO");
                ModelState.Remove("Usuario");
                ModelState.Remove("Detalles");
                ModelState.Remove("DetallePedidos");

                if (ModelState.IsValid)
                {
                    int idGenerado = _logicaPedido.Crear(nuevoPedido);
                    TempData["MensajeExito"] = $"¡Pedido #{idGenerado} creado correctamente!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var listaErrores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    ViewBag.Error = "Revise los campos: " + string.Join(" | ", listaErrores);
                }
            }
            catch (ArgumentException argEx)
            {
                ViewBag.Error = argEx.Message;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en el servidor: " + ex.Message;
            }

            CargarUsuariosDropdown();
            return View(nuevoPedido);
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