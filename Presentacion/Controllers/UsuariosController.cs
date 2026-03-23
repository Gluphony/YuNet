using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LogicaNegocio;
using System;
using System.Data;

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

        // 3. EDITAR USUARIO
        [HttpPost]
        public IActionResult Editar(int Id_Usuario, string Nombre, string Correo, int Id_Rol, string Estado)
        {
            try
            {
                // ==========================================
                // 1. OBTENER EL CARNET (ID DEL USUARIO LOGUEADO)
                // ==========================================
                // Dependiendo de tu Login, descomenta la opción que uses:

                // OPCIÓN A (Si usas Identity o Autenticación por Cookies/Claims):
                // int idLogueado = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                // OPCIÓN B (Si usas Variables de Sesión tradicionales):
                // int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

                // -> Pon aquí tu variable real:
                int idLogueado = 1; // (Borra este 1 y usa tu Opción A o B)

                // ==========================================
                // 2. CONVERSIÓN Y LLAMADA A LA LÓGICA
                // ==========================================
                bool boolEstado = (Estado == "1" || Estado == "true" || Estado == "True");

                // ¡Aquí le pasamos el idLogueado al final!
                bool respuesta = _logicaUsuario.EditarUsuario(Id_Usuario, Nombre, Correo, Id_Rol, boolEstado, idLogueado);

                if (respuesta)
                    TempData["Exito"] = "Usuario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                // Esto atrapará el RAISERROR de SQL ("No puedes desactivarte a ti mismo") 
                // y lo mostrará en tu vista gracias al TempData.
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