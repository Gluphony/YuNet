using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <-- NECESARIO PARA LA SEGURIDAD
using Entidades;
using LogicaNegocio;
using System;
using System.Data;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO BASE: Solo Administradores y Auditores pueden entrar a la sección de Roles
    [Authorize(Roles = "Administrador,Admin,Auditor")]
    public class RolController : Controller
    {
        private readonly LogicaRol _logicaRol = new LogicaRol();

        // 1. VISTA PRINCIPAL
        // Administrador y Auditor pueden ver la tabla
        [HttpGet]
        public IActionResult Index()
        {
            DataTable tablaRoles = new DataTable();
            try
            {
                tablaRoles = _logicaRol.ListarRoles();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los roles: " + ex.Message;
            }

            return View(tablaRoles);
        }

        // 2. CREAR ROL
        // 🔥 ESCUDO ESTRICTO: Auditor NO puede crear roles
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
        public IActionResult Crear(Rol objRol)
        {
            try
            {
                bool respuesta = _logicaRol.InsertarRol(objRol);
                if (respuesta)
                    TempData["Exito"] = "Rol creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 3. EDITAR ROL
        // 🔥 ESCUDO ESTRICTO: Auditor NO puede editar roles
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
        public IActionResult Editar(Rol objRol, string VersionOriginalBase64, string EstadoHtml)
        {
            try
            {
                // Mapeo del estado que viene del formulario HTML
                objRol.ESTADO = (EstadoHtml == "1" || EstadoHtml == "true" || EstadoHtml == "True");

                // Convertimos el string Base64 que vino del HTML de regreso a un byte[]
                byte[] versionOriginal = null;
                if (!string.IsNullOrEmpty(VersionOriginalBase64))
                {
                    versionOriginal = Convert.FromBase64String(VersionOriginalBase64);
                }

                bool respuesta = _logicaRol.EditarRol(objRol, versionOriginal);

                if (respuesta)
                    TempData["Exito"] = "Rol actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 4. DESACTIVAR ROL
        // 🔥 ESCUDO ESTRICTO: Auditor NO puede desactivar roles
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
        public IActionResult Desactivar(int idRol)
        {
            try
            {
                bool respuesta = _logicaRol.DesactivarRol(idRol);
                if (respuesta)
                    TempData["Exito"] = "Rol desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}