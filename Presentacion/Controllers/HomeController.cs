using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Presentacion.Models;

namespace Presentacion.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

            private readonly ILogger<HomeController> _logger;

            // El constructor se queda limpio, solo para inyectar dependencias
            public HomeController(ILogger<HomeController> logger)
            {
                _logger = logger;
            }

            // 2. PANTALLA PRINCIPAL
            public IActionResult Index()
            {
                ViewBag.NombreUsuario = User.Identity.Name;
                return View();
            }

            public IActionResult Privacy()
            {
                return View();
            }

            // 3. PANTALLA DE ACCESO DENEGADO (Si un usuario intenta entrar a un módulo que no es de su rol)
            [AllowAnonymous] // Permite ver esto aunque no tengan permisos
            public IActionResult AccesoDenegado()
            {
                return View();
            }

            // 4. PANTALLA DE ERROR
            [AllowAnonymous] // Es buena práctica que los errores siempre se puedan mostrar
            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }