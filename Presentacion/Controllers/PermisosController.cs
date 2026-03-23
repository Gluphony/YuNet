using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LogicaNegocio;
using Entidades;
using Microsoft.AspNetCore.Authorization;

namespace Presentacion.Controllers
{
    // Solo administradores deberían gestionar la tabla maestra de permisos
    [Authorize(Roles = "Administrador")]
    public class PermisosController : Controller
    {
        private readonly LogicaPermisos _logicaPermisos = new LogicaPermisos();

        // GET: Permisos
        // Lista todos los permisos registrados
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var lista = _logicaPermisos.ListarPermisos();
                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los permisos: " + ex.Message;
                return View(new List<Permisos>());
            }
        }

        // GET: Permisos/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Permisos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Permisos oPermiso)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool respuesta = _logicaPermisos.CrearPermiso(oPermiso);
                    if (respuesta)
                    {
                        TempData["Success"] = "Permiso creado correctamente.";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(oPermiso);
        }

        // GET: Permisos/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                // Buscamos el permiso en la lista por su ID
                var permiso = _logicaPermisos.ListarPermisos()
                                .FirstOrDefault(p => p.ID_PERMISO == id);

                if (permiso == null) return NotFound();

                return View(permiso);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Permisos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Permisos oPermiso)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool respuesta = _logicaPermisos.EditarPermiso(oPermiso);
                    if (respuesta)
                    {
                        TempData["Success"] = "Permiso actualizado correctamente.";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(oPermiso);
        }

        // POST: Permisos/Desactivar/5
        [HttpPost]
        public JsonResult Desactivar(int id)
        {
            try
            {
                bool exito = _logicaPermisos.DesactivarPermiso(id);
                return Json(new { success = exito, message = exito ? "Permiso desactivado." : "No se pudo desactivar." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}