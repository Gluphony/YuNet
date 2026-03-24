using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Presentacion.Models;
using Microsoft.Extensions.Logging;

using LogicaNegocio;
using Entidades;

namespace Presentacion.Controllers
{
    // Quitamos el [Authorize] de aquí arriba para tener control por cada método
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ==========================================
        // 2. PANTALLA PRINCIPAL (ENRUTADOR)
        // ==========================================
        [AllowAnonymous]
        public IActionResult Index()
        {
            // 1. ELIMINAMOS LAS REDIRECCIONES
            // Ahora cualquier persona (visitante, cliente, vendedor, admin) puede entrar aquí.

            // 2. LÓGICA EXCLUSIVA PARA ADMINISTRADORES
            // Solo hacemos las consultas pesadas a la base de datos si el usuario es Admin.
            if (User.Identity.IsAuthenticated && (User.IsInRole("Administrador") || User.IsInRole("Admin")))
            {
                ViewBag.NombreUsuario = User.Identity.Name;

                try
                {
                    DataTable dtProductos = _logicaProducto.ListarProductos();
                    List<Producto> listaProductos = new List<Producto>();

                    foreach (DataRow row in dtProductos.Rows)
                    {
                        listaProductos.Add(new Producto
                        {
                            ID_PRODUCTO = Convert.ToInt32(row["ID_PRODUCTO"]),
                            MODELO = row["MODELO"].ToString(),
                            PRECIO = Convert.ToDecimal(row["PRECIO"]),
                            EXISTENCIAS = Convert.ToInt32(row["EXISTENCIAS"]),
                            ESTADO = Convert.ToBoolean(row["ESTADO"])
                        });
                    }

                    var productosActivos = listaProductos.Where(p => p.ESTADO).ToList();

                    ViewBag.TotalProductos = productosActivos.Count;
                    ViewBag.ProductosStockBajo = productosActivos.Count(p => p.EXISTENCIAS <= 5);

                    var topProductos = productosActivos.Take(4).Select(p => new {
                        Categoria = "Equipo",
                        Nombre = p.MODELO,
                        Stock = p.EXISTENCIAS,
                        Precio = p.PRECIO
                    }).ToList();

                    ViewBag.ProductosPopulares = topProductos;

                    ViewBag.TotalUsuarios = 8;
                    ViewBag.VentasHoy = 14;
                    ViewBag.IngresosMes = 12450.00m;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error cargando el Dashboard: " + ex.Message);
                    ViewBag.TotalProductos = 0;
                    ViewBag.ProductosStockBajo = 0;
                    ViewBag.TotalUsuarios = 0;
                    ViewBag.VentasHoy = 0;
                    ViewBag.IngresosMes = 0.0m;
                }
            }

            // 3. Retorna la vista (Index.cshtml)
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Cualquier usuario logueado puede cerrar sesión
        [Authorize]
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}