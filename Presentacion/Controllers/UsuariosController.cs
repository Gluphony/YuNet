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
                // El checkbox o select del formulario HTML suele devolver "true"/"false" o "1"/"0"
                // Aquí lo convertimos a booleano de forma segura
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