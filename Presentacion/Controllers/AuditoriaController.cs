using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio; // Asegúrate de que este sea el namespace correcto de tu capa lógica

namespace Presentacion.Controllers
{
    [Authorize]
    public class AuditoriaController : Controller
    {

        private readonly logicaAuditoria _logicaAuditoria = new logicaAuditoria();

        // 1. VISTA PRINCIPAL
        public IActionResult Index()
        {
            // Llamamos a la Lógica
            var listaAuditoria = _logicaAuditoria.ListarAuditoriaGeneral();
            return View(listaAuditoria);
        }

        // 2. FILTRAR POR FECHA
        [HttpPost]
        public IActionResult FiltrarPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                ViewBag.Error = "La fecha de inicio no puede ser mayor a la fecha final.";
                return View("Index", _logicaAuditoria.ListarAuditoriaGeneral());
            }

            // Llamamos a la Lógica
            var listaFiltrada = _logicaAuditoria.ListarAuditoriaPorFecha(fechaInicio, fechaFin);

            ViewBag.FechaInicio = fechaInicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.ToString("yyyy-MM-dd");

            return View("Index", listaFiltrada);
        }
    }
}