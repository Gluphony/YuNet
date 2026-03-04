using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using LogicaNegocio; // Referencia a tu capa lógica

namespace Presentacion.Controllers
{
    public class LoginController : Controller
    {
        // Instanciamos la capa lógica de seguridad
        private readonly LogicaLogin _logicaSeguridad = new LogicaLogin();

        // 1. GET: Muestra la pantalla de Login
        [HttpGet]
        public IActionResult Index()
        {
            // Si el usuario ya está logueado, lo mandamos directo al Inicio
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // 2. POST: Procesa las credenciales
        [HttpPost]
        public IActionResult Index(string correo, string clave)
        {
            try
            {
                // Llamamos a nuestra capa lógica
                DataTable dtUsuario = _logicaSeguridad.ValidarLogin(correo, clave);

                if (dtUsuario != null && dtUsuario.Rows.Count > 0)
                {
                    // ¡Login Exitoso! Extraemos los datos del DataTable
                    DataRow fila = dtUsuario.Rows[0];
                    string idUsuario = fila["Id_Usuario"].ToString();
                    string nombre = fila["Nombre"].ToString();
                    string rol = fila["Nombre_Rol"].ToString(); // Viene de tu INNER JOIN
                    string correoDb = fila["Correo"].ToString();

                    // Creamos los "Claims" (credenciales del usuario para la sesión)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, idUsuario),
                        new Claim(ClaimTypes.Name, nombre),
                        new Claim(ClaimTypes.Email, correoDb),
                        new Claim(ClaimTypes.Role, rol) // Fundamental para proteger vistas por rol
                    };

                    // Creamos la identidad y firmamos la cookie
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true // Para mantener la sesión si cierra el navegador (opcional)
                    };

                    // Iniciar sesión en el sistema
                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    ).Wait();

                    // Redirigimos al panel principal (Home/Index)
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Login fallido: Cero filas devueltas
                    ViewBag.Error = "Correo o contraseña incorrectos.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Mostramos el error de la capa lógica (ej. "El correo es obligatorio")
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // 3. GET: Cerrar Sesión
        public IActionResult Salir()
        {
            // Destruimos la cookie de sesión
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

            // Redirigimos al Login
            return RedirectToAction("Index", "Login");
        }
    }
}