using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Presentacion.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolPermisosController : Controller
    {
        private readonly LogicaRolPermisos _logicaRolPermisos = new LogicaRolPermisos();
        private readonly LogicaRol _logicaRol = new LogicaRol();

        [HttpGet]
        public IActionResult Index(int? idRolSeleccionado)
        {
            // 1. Cargamos los Roles para el dropdown (DataTable)
            ViewBag.Roles = _logicaRol.ListarRoles();

            // 2. Cargamos el catálogo completo (Entidad: Permisos)
            // Este método en tu lógica debe devolver List<Permisos>
            ViewBag.TodosLosPermisos = _logicaRolPermisos.ListarTodos();

            ViewBag.IdRolActual = idRolSeleccionado;

            // 3. Permisos que el rol ya tiene asignados (Lista para comparar)
            // Se asume que ListarPermisosPorRol devuelve una lista de objetos que contienen ID_PERMISO
            List<Permisos> permisosAsignados = new List<Permisos>();

            if (idRolSeleccionado.HasValue && idRolSeleccionado.Value > 0)
            {
                try
                {
                    permisosAsignados = _logicaRolPermisos.ListarPermisosPorRol(idRolSeleccionado.Value);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al cargar permisos: " + ex.Message;
                }
            }

            return View(permisosAsignados);
        }

        [HttpPost]
        public IActionResult AsignarPermiso(int idRol, int idPermiso)
        {
            try
            {
                // 1. Validaciones básicas de seguridad antes de llamar a la lógica
                if (idRol <= 0 || idPermiso <= 0)
                {
                    TempData["Error"] = "Datos no válidos para la asignación.";
                    return RedirectToAction("Index");
                }

                // 2. Llamamos a tu lógica de negocio
                bool resultado = _logicaRolPermisos.AsignarPermiso(idRol, idPermiso);

                if (resultado)
                {
                    TempData["Exito"] = "Permiso asignado correctamente.";
                }
                else
                {
                    // Esto sucede si el SP devolvió 0 (ej: ya existía el permiso)
                    TempData["Error"] = "El permiso ya está asignado a este rol o no se pudo procesar.";
                }
            }
            catch (Exception ex)
            {
                // Atrapamos el error de la lógica o de SQL
                TempData["Error"] = "Error: " + ex.Message;
            }

            // 3. LA CLAVE: Redireccionamos al Index pasando el ID del Rol 
            // para que la vista se cargue mostrando los cambios de ese rol específico.
            return RedirectToAction("Index", new { idRolSeleccionado = idRol });
        }

        [HttpPost]
        public IActionResult RevocarPermiso(int idRol, int idPermiso)
        {
            try
            {
                // 1. Validaciones de entrada (Capa de seguridad extra)
                if (idRol <= 0 || idPermiso <= 0)
                {
                    TempData["Error"] = "ID de Rol o Permiso no válidos para la operación.";
                    return RedirectToAction("Index");
                }

                // 2. Llamada a tu Capa de Negocio
                bool eliminado = _logicaRolPermisos.RevocarPermiso(idRol, idPermiso);

                if (eliminado)
                {
                    TempData["Exito"] = "El permiso ha sido revocado correctamente.";
                }
                else
                {
                    // Esto sucede si el ExecuteNonQuery devolvió 0 (el permiso no estaba asignado)
                    TempData["Error"] = "No se pudo revocar el permiso. Es posible que ya no estuviera asignado.";
                }
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error de SQL o lógica
                TempData["Error"] = "Error al revocar: " + ex.Message;
            }

            // 3. REDIRECCIÓN: Volvemos al Index manteniendo el Rol seleccionado 
            // para que la vista recargue los permisos actuales.
            return RedirectToAction("Index", new { idRolSeleccionado = idRol });
        }

        [HttpPost]
        public IActionResult Guardar(int idRol, List<int> permisosSeleccionados)
        {
            if (idRol <= 0)
            {
                TempData["Error"] = "Seleccione un rol válido.";
                return RedirectToAction("Index");
            }

            try
            {
                // Obtenemos los IDs actuales para comparar
                var actuales = _logicaRolPermisos.ListarPermisosPorRol(idRol)
                                                 .Select(p => p.ID_PERMISO)
                                                 .ToList();

                permisosSeleccionados = permisosSeleccionados ?? new List<int>();

                // ASIGNAR: Nuevos seleccionados que no estaban en la base
                foreach (var id in permisosSeleccionados.Except(actuales))
                {
                    _logicaRolPermisos.AsignarPermiso(idRol, id);
                }

                // REVOCAR: Estaban en la base pero ya no se seleccionaron
                foreach (var id in actuales.Except(permisosSeleccionados))
                {
                    _logicaRolPermisos.RevocarPermiso(idRol, id);
                }

                TempData["Exito"] = "Permisos actualizados correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar: " + ex.Message;
            }

            return RedirectToAction("Index", new { idRolSeleccionado = idRol });
        }
    }
}