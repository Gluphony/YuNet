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
        [HttpPost]
        public IActionResult CrearPermiso(string CLAVE_PERMISO, string DESCRIPCION)
        {
            try
            {
                // Creamos el objeto con los datos del formulario
                Permisos nuevoPermiso = new Permisos
                {
                    CLAVE_PERMISO = CLAVE_PERMISO,
                    DESCRIPCION = DESCRIPCION
                };

                // Llamamos a la lógica (Asegúrate de haber creado este método en tu capa Lógica y Datos)
                _logicaRolPermisos.InsertarPermiso(nuevoPermiso);

                TempData["Exito"] = "Permiso creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el permiso: " + ex.Message;
            }

            // Recargamos la misma página
            return RedirectToAction("Index");
        }
    }
}