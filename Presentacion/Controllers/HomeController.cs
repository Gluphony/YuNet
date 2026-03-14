using System.Diagnostics;
using System.Threading.Tasks; // <-- 1. Agregado para tareas asíncronas
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication; // <-- 2. Agregado para cerrar sesión
using Microsoft.AspNetCore.Authentication.Cookies; // <-- 3. Agregado para limpiar la cookie
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

        // ==========================================
        // 5. CERRAR SESIÓN
        // ==========================================
        public async Task<IActionResult> Salir()
        {
            // Destruye la cookie de autenticación del usuario
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige al usuario a la pantalla de Login (Asumiendo que tu controlador es 'Login' y la vista 'Index')
            return RedirectToAction("Index", "Login");
        }
    }
}