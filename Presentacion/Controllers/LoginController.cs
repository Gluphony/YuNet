using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using LogicaNegocio;

namespace Presentacion.Controllers
{
    public class LoginController : Controller
    {
        private readonly LogicaLogin _logicaSeguridad = new LogicaLogin();

        [HttpGet]
        public IActionResult Index()
        {
            // 💡 MEJORA: Prevenir NullReferenceException
            // Verificamos primero si User e Identity no son nulos antes de preguntar si está autenticado
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                // Si ya está logueado, lo devolvemos a la tienda/home, NO al login de nuevo
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string correo, string clave)
        {
            try
            {
                DataTable dtUsuario = _logicaSeguridad.ValidarLogin(correo, clave);

                if (dtUsuario != null && dtUsuario.Rows.Count > 0)
                {
                    DataRow fila = dtUsuario.Rows[0];

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, fila["Id_Usuario"].ToString()),
                        new Claim(ClaimTypes.Name, fila["Nombre"].ToString()),
                        new Claim(ClaimTypes.Email, fila["Correo"].ToString()),
                        new Claim(ClaimTypes.Role, fila["Nombre_Rol"].ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    );

                    // Login exitoso -> Lo mandamos al Home
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Correo o contraseña incorrectos.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error en el servidor: " + ex.Message;
                return View();
            }
        }

        // 💡 MEJORA: El método Salir() debe ser accesible por GET o POST, 
        // normalmente se usa GET desde un enlace en el Layout.
        [HttpGet]
        public async Task<IActionResult> Salir()
        {
            // Cerramos la sesión eliminando la cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 💡 MEJORA DE EXPERIENCIA: Al salir, lo mandamos a la tienda como visitante, 
            // no lo forzamos a ver la pantalla de Login otra vez (opcional, pero recomendado).
            return RedirectToAction("Index", "Home");
        }
    }
}