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
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();

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
                    string rolUsuario = fila["Nombre_Rol"].ToString();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, fila["Id_Usuario"].ToString()),
                        new Claim(ClaimTypes.Name, fila["Nombre"].ToString()),
                        new Claim(ClaimTypes.Email, fila["Correo"].ToString()),
                        new Claim(ClaimTypes.Role, rolUsuario)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // =========================================================
                    // 1. LÓGICA DE TIEMPOS DE SESIÓN INDEPENDIENTES POR ROL
                    // =========================================================
                    DateTimeOffset tiempoExpiracion;
                    bool permitirRefresco;
                    bool esPersistente;

                    if (rolUsuario == "Usuario")
                    {
                        tiempoExpiracion = DateTimeOffset.UtcNow.AddMinutes(15); // 15 Minutos límite
                        permitirRefresco = false; // No se renueva el tiempo al navegar
                        esPersistente = false; // Se cierra si apagan el navegador
                    }
                    else if (rolUsuario == "Vendedor")
                    {
                        tiempoExpiracion = DateTimeOffset.UtcNow.AddHours(1); // 1 Hora límite
                        permitirRefresco = false;
                        esPersistente = false;
                    }
                    else
                    {
                        // Administradores, Auditores y otros
                        tiempoExpiracion = DateTimeOffset.UtcNow.AddHours(8); // 8 Horas
                        permitirRefresco = true; // Se renueva si siguen activos
                        esPersistente = true; // Se mantiene iniciada
                    }

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = permitirRefresco,
                        IsPersistent = esPersistente,
                        ExpiresUtc = tiempoExpiracion
                    };
                    // =========================================================

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

        [HttpGet]
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

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
                bool exito = _logicaUsuario.RestablecerClave(Correo, NuevaClave);

                if (exito)
                {
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