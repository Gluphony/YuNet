using System;
using System.Linq;
using System.Data;
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

        public CarritoController() { }

        // --- MÉTODO AUXILIAR: Extraer el ID del usuario ---
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

                    DataView vistaProductos = new DataView(todosLosProductos);
                    vistaProductos.RowFilter = "ESTADO = 1"; // Ajusta si tu columna se llama diferente (ej: ACTIVO = 1)
                    ViewBag.ProductosDisponibles = vistaProductos.ToTable();

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
        // 3. PROCESAR COMPRA (CLIENTE NORMAL)
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
        // VISTA DE CONFIRMACIÓN DE PEDIDO
        // ==========================================
        [HttpGet]
        public IActionResult Confirmacion(int idPedido)
        {
            ViewBag.IdPedido = idPedido;
            return View();
        }
    }
}