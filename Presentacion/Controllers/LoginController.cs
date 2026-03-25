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
        // Instanciamos la lógica
        private readonly LogicaLogin _logicaSeguridad = new LogicaLogin();
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();

        // =================================================================
        // LOGIN
        // =================================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
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

        // =================================================================
        // SALIR (CERRAR SESIÓN)
        // =================================================================
        [HttpGet]
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // =================================================================
        // REGISTRO
        // =================================================================
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(string Nombre, string Correo, string Clave, int Id_Rol)
        {
            try
            {
                bool respuesta = _logicaUsuario.InsertarUsuario(Nombre, Correo, Clave, Id_Rol);

                if (respuesta)
                {
                    ViewBag.Exito = "¡Cuenta creada con éxito! Ahora puedes iniciar sesión.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        // =================================================================
        // RESTABLECER CLAVE
        // =================================================================
        [HttpGet]
        public IActionResult RestablecerClave()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RestablecerClave(string Correo, string NuevaClave)
        {
            try
            {
                // Asegúrate de tener el método RestablecerClave en tu LogicaUsuario y Base de Datos
                bool exito = _logicaUsuario.RestablecerClave(Correo, NuevaClave);

                if (exito)
                {
                    // Si cambia la clave, mandamos un mensaje de éxito al Login
                    ViewBag.Exito = "¡Contraseña actualizada! Ya puedes iniciar sesión.";
                    return View("Index");
                }
                else
                {
                    TempData["Error"] = "El correo ingresado no pertenece a ningún usuario activo.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return View();
        }
    }
}