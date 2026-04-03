using Entidades;
using LogicaNegocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization; // <-- NECESARIO PARA LA SEGURIDAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO BASE: Tienes que haber iniciado sesión para tocar cualquier cosa de pedidos
    [Authorize]
    public class PedidoController : Controller
    {
        // Instancias de tus capas de Lógica de Negocio
        private readonly LogicaPedido _logicaPedido = new LogicaPedido();
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();
        private readonly LogicaDetallePedido _logicaDetalle = new LogicaDetallePedido();

        // ==========================================
        // GET: Pedido/Index (Tabla general para el Admin)
        // ==========================================
        // 🔥 ESCUDO: Solo el Staff operativo y de auditoría pueden ver todos los pedidos del negocio
        [Authorize(Roles = "Administrador,Admin,Vendedor,Auditor")]
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
        // GET: Pedido/Historial (Pedidos propios)
        // ==========================================
        // Todos los logueados pueden entrar (Clientes y Staff)
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
                    return RedirectToAction("Index", "Login"); // Ajustado a "Index" de "Login"
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

        // ==========================================
        // GET: Pedido/Details
        // ==========================================
        // Todos pueden ver detalles (El cliente ve los suyos, el admin ve todos)
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
        // 🔥 ESCUDO ESTRICTO: Auditor y Clientes NO pueden asignar IMEIs
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
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
        // GET: Pedido/Ticket
        // ==========================================
        // Todos pueden ver el ticket
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

                // 1. Obtenemos la info general del pedido
                var infoPedido = _logicaPedido.ObtenerPorId(id);
                ViewBag.InfoPedido = infoPedido;
                ViewBag.NumeroPedido = id;

                // 2. Buscamos el correo del cliente
                string correoCliente = "No registrado";
                try
                {
                    var dtUsuarios = _logicaUsuario.ListarUsuarios();
                    foreach (System.Data.DataRow row in dtUsuarios.Rows)
                    {
                        string nombreBd = row.Table.Columns.Contains("NOMBRE") ? row["NOMBRE"].ToString() : "";
                        int idBd = row.Table.Columns.Contains("ID_USUARIO") && row["ID_USUARIO"] != DBNull.Value ? Convert.ToInt32(row["ID_USUARIO"]) : 0;

                        if (idBd == infoPedido.ID_USUARIO || nombreBd == infoPedido.CLIENTE)
                        {
                            if (row.Table.Columns.Contains("CORREO")) correoCliente = row["CORREO"].ToString();
                            else if (row.Table.Columns.Contains("Correo")) correoCliente = row["Correo"].ToString();
                            break;
                        }
                    }
                }
                catch { } // Si falla la búsqueda del correo, no pasa nada, se queda como "No registrado"

                ViewBag.CorreoCliente = correoCliente;

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
        // 🔥 ESCUDO ESTRICTO: Auditor y Clientes NO pueden cambiar el estado del pedido
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
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