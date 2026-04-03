using System;
using System.Collections.Generic;
using System.Linq; // Agregado para el FirstOrDefault
using Microsoft.AspNetCore.Mvc;
using LogicaNegocio;
using Entidades;
using Microsoft.AspNetCore.Authorization;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO BASE: Solo el Administrador y el Auditor pueden entrar a ver la tabla.
    [Authorize(Roles = "Administrador,Admin,Auditor")]
    public class PermisosController : Controller
    {
        private readonly LogicaPermisos _logicaPermisos = new LogicaPermisos();

        // GET: Permisos
        // Todos los autorizados arriba pueden ver la lista.
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
        // 🔥 ESCUDO: El Auditor queda fuera de la creación.
        [HttpGet]
        [Authorize(Roles = "Administrador,Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Permisos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Admin")]
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
        // 🔥 ESCUDO: El Auditor queda fuera de la edición.
        [HttpGet]
        [Authorize(Roles = "Administrador,Admin")]
        public ActionResult Edit(int id)
        {
            try
            {
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
        [Authorize(Roles = "Administrador,Admin")]
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
        // 🔥 ESCUDO: El Auditor queda fuera de la desactivación.
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
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