using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization; // <-- Necesario para las etiquetas de seguridad
using LogicaNegocio;

namespace Presentacion.Controllers
{
    // El controlador base no lleva Authorize porque es la puerta de entrada
    public class LoginController : Controller
    {
        private readonly LogicaLogin _logicaSeguridad = new LogicaLogin();
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();

        // ========================================================
        // 1. INICIAR SESIÓN (Público)
        // ========================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Si el usuario ya está logueado, lo mandamos directo al Home
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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
                    // LÓGICA DE TIEMPOS DE SESIÓN INDEPENDIENTES POR ROL
                    // =========================================================
                    DateTimeOffset tiempoExpiracion;
                    bool permitirRefresco;
                    bool esPersistente;

                    if (rolUsuario == "Usuario")
                    {
                        tiempoExpiracion = DateTimeOffset.UtcNow.AddMinutes(15); // 15 Minutos límite
                        permitirRefresco = false;
                        esPersistente = false;
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
                        permitirRefresco = true;
                        esPersistente = true;
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

        // ========================================================
        // 2. CERRAR SESIÓN (Solo usuarios autenticados)
        // ========================================================
        [HttpGet]
        [Authorize] // 🔥 Solo puedes salir si estás adentro
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ========================================================
        // 3. REGISTRO DE USUARIOS (Público)
        // ========================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(string Nombre, string Correo, string Clave, int Id_Rol)
        {
            try
            {
                // A. Validar longitud de la contraseña
                if (string.IsNullOrEmpty(Clave) || Clave.Length < 5)
                {
                    ViewBag.Error = "La contraseña debe tener al menos 5 caracteres.";
                    ViewBag.NombreTemp = Nombre;
                    ViewBag.CorreoTemp = Correo;
                    return View();
                }

                // B. Validar formato de correo según el rol
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
                    return View();
                }

                // C. Ejecución en base de datos
                bool respuesta = _logicaUsuario.InsertarUsuario(Nombre, Correo, Clave, Id_Rol);

                if (respuesta)
                {
                    ViewBag.Exito = "¡Cuenta creada con éxito! Ahora puedes iniciar sesión.";
                    return View("Index");
                }
                else
                {
                    ViewBag.Error = "No se pudo registrar el usuario. Verifique los datos.";
                    ViewBag.NombreTemp = Nombre;
                    ViewBag.CorreoTemp = Correo;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.NombreTemp = Nombre;
                ViewBag.CorreoTemp = Correo;
            }

            return View();
        }

        // ========================================================
        // 4. RESTABLECER CONTRASEÑA (Público)
        // ========================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RestablecerClave()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult RestablecerClave(string Correo, string NuevaClave)
        {
            try
            {
                bool exito = _logicaSeguridad.CambiarClave(Correo, NuevaClave);

                if (exito)
                {
                    TempData["Exito"] = "¡Contraseña actualizada! Ya puedes iniciar sesión.";
                    return RedirectToAction("Index", "Login");
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
    }
}