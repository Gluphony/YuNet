using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using LogicaNegocio;
using System;
using System.Data;

namespace Presentacion.Controllers
{
    // Candado: Solo Administrador
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();

        private int ObtenerIdUsuarioActual()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId != null && int.TryParse(claimId.Value, out int id))
            {
                return id;
            }
            return 0;
        }

        private bool EsElUltimoAdministradorActivo(int idUsuarioAfectado)
        {
            DataTable tablaUsuarios = _logicaUsuario.ListarUsuarios();
            int cantidadAdminsActivos = 0;
            bool esElUsuarioAfectadoAdmin = false;

            foreach (DataRow fila in tablaUsuarios.Rows)
            {
                bool estado = fila["Estado"] != DBNull.Value && Convert.ToBoolean(fila["Estado"]);
                int idRol = fila["Id_Rol"] != DBNull.Value ? Convert.ToInt32(fila["Id_Rol"]) : 0;
                int idUser = Convert.ToInt32(fila["Id_Usuario"]);

                if (estado && idRol == 1)
                {
                    cantidadAdminsActivos++;
                    if (idUser == idUsuarioAfectado)
                    {
                        esElUsuarioAfectadoAdmin = true;
                    }
                }
            }
            return esElUsuarioAfectadoAdmin && cantidadAdminsActivos <= 1;
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
                    TempData["Exito"] = "¡Contraseña actualizada! Ya puedes iniciar sesión.";
                    return RedirectToAction("Index");
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

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.IdUsuarioActual = ObtenerIdUsuarioActual();
            DataTable tablaUsuarios = new DataTable();
            try
            {
                tablaUsuarios = _logicaUsuario.ListarUsuarios();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los usuarios: " + ex.Message;
            }
            return View(tablaUsuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(string Nombre, string Correo, string Clave, int Id_Rol)
        {
            try
            {
                bool respuesta = _logicaUsuario.InsertarUsuario(Nombre, Correo, Clave, Id_Rol);
                if (respuesta)
                {
                    // Guardamos el mensaje de éxito para cuando la página se recargue
                    TempData["Exito"] = "Usuario registrado correctamente.";
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "No se pudo registrar el usuario." });
            }
            catch (Exception ex)
            {
                // Devolvemos el error de la base de datos en formato JSON
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int Id_Usuario, string Nombre, string Correo, int Id_Rol, bool Estado)
        {
            try
            {
                // 1. OBTENER EL ID DEL USUARIO LOGUEADO DESDE LOS CLAIMS
                // Buscamos el claim donde guardaste el ID al momento del Login (usualmente NameIdentifier)
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Lo convertimos a entero de forma segura. Si por alguna razón no lo encuentra, asignará 0.
                int idUsuarioLogueado = string.IsNullOrEmpty(claimId) ? 0 : int.Parse(claimId);

                // 2. PASAR EL SEXTO PARÁMETRO AL MÉTODO DE LÓGICA
                bool respuesta = _logicaUsuario.EditarUsuario(Id_Usuario, Nombre, Correo, Id_Rol, Estado, idUsuarioLogueado);

                if (respuesta)
                {
                    TempData["Exito"] = "Usuario actualizado correctamente.";
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "No se pudo actualizar el usuario." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Desactivar(int Id_Usuario)
        {
            try
            {
                // 1. Sacamos el ID del que está logueado desde los Claims
                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int idLogueado = string.IsNullOrEmpty(claimId) ? 0 : int.Parse(claimId);

                // 2. Llamamos a la lógica
                bool respuesta = _logicaUsuario.DesactivarUsuario(Id_Usuario, idLogueado);

                if (respuesta)
                {
                    return Json(new { success = true, message = "Usuario desactivado correctamente." });
                }
                return Json(new { success = false, message = "No se pudo realizar la operación." });
            }
            catch (Exception ex)
            {
                // El mensaje que viene del THROW de SQL llegará aquí
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}