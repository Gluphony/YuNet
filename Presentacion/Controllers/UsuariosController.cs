using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LogicaNegocio;
using System;
using System.Data;
using System.Security.Claims;

namespace Presentacion.Controllers
{
    // Solo el administrador gestiona usuarios
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();

        // 1. VISTA PRINCIPAL
        [HttpGet]
        public IActionResult Index()
        {
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

        // 2. CREAR USUARIO
        [HttpPost]
        public IActionResult Crear(string Nombre, string Correo, string Clave, int idRol)
        {
            try
            {
                // SOLUCIÓN: Necesitamos el ID del Rol de la persona que está ejecutando esta acción.
                // Como este controlador tiene [Authorize(Roles = "Administrador")], sabemos que es un Admin.
                // Asumiendo que el ID del rol Administrador en tu BD es 1:
                int idRolDelUsuarioCreador = 1;

                // *Nota Pro*: Si guardas el IdRol en los Claims durante el Login, lo correcto sería leerlo así:
                // string claimRol = User.FindFirst("IdRol")?.Value;
                // int idRolDelUsuarioCreador = claimRol != null ? int.Parse(claimRol) : 1;

                // Ahora sí pasamos los 5 parámetros que exige la capa lógica
                bool respuesta = _logicaUsuario.InsertarUsuario(idRolDelUsuarioCreador, Nombre, Correo, Clave, idRol);

                if (respuesta)
                    TempData["Exito"] = "Usuario registrado correctamente.";
            }
            catch (Exception ex)
            {
                // Si la validación de la contraseña falla (ej. no tiene mayúsculas), 
                // el Exception caerá aquí y le mostrará el error al usuario en la vista.
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        // 3. EDITAR USUARIO
        [HttpPost]
        public IActionResult Editar(int Id_Usuario, string Nombre, string Correo, int Id_Rol, string Estado)
        {
            try
            {
                bool boolEstado = (Estado == "1" || Estado == "true" || Estado == "True");

                bool respuesta = _logicaUsuario.EditarUsuario(Id_Usuario, Nombre, Correo, Id_Rol, boolEstado);
                if (respuesta)
                    TempData["Exito"] = "Usuario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        // 4. DESACTIVAR USUARIO
        [HttpPost]
        public IActionResult Desactivar(int idUsuario)
        {
            try
            {
                bool respuesta = _logicaUsuario.DesactivarUsuario(idUsuario);
                if (respuesta)
                    TempData["Exito"] = "Usuario desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}