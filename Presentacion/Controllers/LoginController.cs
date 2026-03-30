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
                // ==========================================
                // 1. VALIDACIONES EN C# (Backend)
                // ==========================================

                // A. Validar longitud de la contraseña
                if (string.IsNullOrEmpty(Clave) || Clave.Length < 5)
                {
                    ViewBag.Error = "La contraseña debe tener al menos 5 caracteres.";
                    ViewBag.NombreTemp = Nombre;
                    ViewBag.CorreoTemp = Correo;
                    return View(); // Regresa a la vista mostrando el error
                }

                // B. Validar formato de correo según el rol
                // Ajusta los números 1, 2, 3, 4 según los IDs reales de tus roles
                string terminacionEsperada = Id_Rol switch
                {
                    1 => ".admin@yunet.com",
                    2 => ".usuario@yunet.com",
                    3 => ".vendedor@yunet.com",
                    4 => ".auditor@yunet.com",
                    _ => ""
                };

                if (string.IsNullOrEmpty(terminacionEsperada) || (Correo != null && !Correo.EndsWith(terminacionEsperada)))
                {
                    ViewBag.Error = $"Formato de correo inválido. Para este rol, el correo debe terminar en '{terminacionEsperada}'.";
                    ViewBag.NombreTemp = Nombre;
                    ViewBag.CorreoTemp = Correo;
                    return View(); // Regresa a la vista mostrando el error
                }

                // ==========================================
                // 2. EJECUCIÓN EN BASE DE DATOS
                // ==========================================
                bool respuesta = _logicaUsuario.InsertarUsuario(Nombre, Correo, Clave, Id_Rol);

                if (respuesta)
                {
                    // Si todo sale bien, lo mandamos al Login ("Index")
                    ViewBag.Exito = "¡Cuenta creada con éxito! Ahora puedes iniciar sesión.";
                    return View("Index");
                }
                else
                {
                    // Por si la BD devuelve falso sin lanzar excepción
                    ViewBag.Error = "No se pudo registrar el usuario. Verifique los datos.";
                    ViewBag.NombreTemp = Nombre;
                    ViewBag.CorreoTemp = Correo;
                }
            }
            catch (Exception ex)
            {
                // ==========================================
                // 3. CAPTURA DE ERRORES DE SQL (RAISERROR)
                // ==========================================
                // Si hay un error en la BD (ej. correo duplicado), lo atrapamos aquí
                ViewBag.Error = ex.Message;

                // Mantenemos los datos escritos por el usuario (menos la contraseña por seguridad)
                ViewBag.NombreTemp = Nombre;
                ViewBag.CorreoTemp = Correo;
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
                // CAMBIO: Se cambió _logicaUsuario.RestablecerClave por .CambiarClave
                bool exito = _logicaSeguridad.CambiarClave(Correo, NuevaClave);

                if (exito)
                {
                    // Usamos TempData para que el mensaje sobreviva al redireccionamiento
                    TempData["Exito"] = "¡Contraseña actualizada! Ya puedes iniciar sesión.";
                    return RedirectToAction("Index", "Login"); // Es mejor redireccionar al Login
                }
                else
                {
                    ViewBag.Error = "El correo ingresado no pertenece a ningún usuario activo.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        //[HttpPost]
        //public IActionResult RestablecerClave(string Correo, string NuevaClave)
        //{
        //    try
        //    {
        //        bool exito = _logicaUsuario.RestablecerClave(Correo, NuevaClave);

        //        if (exito)
        //        {
        //            ViewBag.Exito = "¡Contraseña actualizada! Ya puedes iniciar sesión.";
        //            return View("Index");
        //        }
        //        else
        //        {
        //            TempData["Error"] = "El correo ingresado no pertenece a ningún usuario activo.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //    }

        //    return View();
        //}
    }
}