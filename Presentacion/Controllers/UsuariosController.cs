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
                    TempData["Exito"] = "Usuario registrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(int Id_Usuario, string Nombre, string Correo, int Id_Rol, string Estado)
        {
            try
            {
                // CORREGIDO: Ahora jala el ID real del usuario conectado, no el "1" quemado
                int idLogueado = ObtenerIdUsuarioActual();
                bool boolEstado = (Estado == "1" || Estado == "true" || Estado == "True");

                bool respuesta = _logicaUsuario.EditarUsuario(Id_Usuario, Nombre, Correo, Id_Rol, boolEstado, idLogueado);

                if (respuesta)
                    TempData["Exito"] = "Usuario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int Id_Usuario)
        {
            try
            {
                int idActual = ObtenerIdUsuarioActual();

                if (EsElUltimoAdministradorActivo(Id_Usuario))
                {
                    TempData["Error"] = "Operación denegada: No puedes desactivar a este usuario porque es el único Administrador activo.";
                    return RedirectToAction("Index");
                }

                bool respuesta = _logicaUsuario.DesactivarUsuario(Id_Usuario, idActual);

                if (respuesta)
                {
                    if (Id_Usuario == idActual)
                    {
                        await HttpContext.SignOutAsync();
                        return RedirectToAction("Index", "Login");
                    }
                    TempData["Exito"] = "Usuario desactivado correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}