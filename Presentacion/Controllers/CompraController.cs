using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Agregado para la seguridad
using LogicaNegocio;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TuProyecto.Controllers // Acordate de cambiar esto por el namespace de tu app
{
    // 🔥 EL ESCUDO BASE: Solo el Staff tiene acceso a esta sección
    [Authorize(Roles = "Administrador,Admin,Vendedor,Auditor")]
    public class CompraController : Controller
    {
        // 1. Instanciamos tus clases con TUS nombres exactos
        private readonly LogicaCompra _logica = new LogicaCompra();
        private readonly Logica_Proveedor _logicaProveedor = new Logica_Proveedor();
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();

        // 2. VISTA PRINCIPAL (Todos los autorizados en la clase pueden ver esto)
        public IActionResult Index()
        {
            try
            {
                // A. Llenamos los combos
                ViewBag.Proveedores = _logicaProveedor.ListarProveedores();
                ViewBag.Productos = _logicaProducto.ListarProductos();

                // B. Llamamos a la base de datos de verdad
                List<Compra> listaCompras = _logica.ListarCompras();

                return View(listaCompras);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar compras: " + ex.Message;
                return View(new List<Compra>());
            }
        }

        // 3. RECIBE LOS DATOS DEL MODAL (AJAX)
        // 🔥 EL ESCUDO ESTRICTO: El Auditor NO puede registrar compras
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult GuardarCompra(int idProveedor, string factura, int idProducto, int cantidad, decimal costo, string listaImeis)
        {
            try
            {
                // Obtenemos el ID del usuario logueado
                int idUsuarioActual = 1; // Valor por defecto

                if (User.Identity.IsAuthenticated)
                {
                    var claimId = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario");
                    if (claimId != null)
                    {
                        idUsuarioActual = Convert.ToInt32(claimId.Value);
                    }
                    // Alternativa segura usando el NameIdentifier estándar:
                    else if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int parsedId))
                    {
                        idUsuarioActual = parsedId;
                    }
                }

                // Armamos el objeto detalle
                DetalleCompra detalle = new DetalleCompra
                {
                    ID_PRODUCTO = idProducto,
                    CANTIDAD = cantidad,
                    COSTO_UNITARIO = costo,
                    LISTA_IMEIS = listaImeis
                };

                // Mandamos a la lógica de compra
                bool exito = _logica.RegistrarEntrada(idProveedor, idUsuarioActual, factura, detalle);

                if (exito)
                {
                    return Json(new { success = true, message = "¡Inventario cargado y stock actualizado!" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo completar el registro." });
                }
            }
            catch (Exception ex)
            {
                // Atrapamos errores (Ej: IMEIs no coinciden con cantidad)
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}