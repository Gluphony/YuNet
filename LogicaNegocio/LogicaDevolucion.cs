using Datos;
using Entidades;
using System;
using System.Collections.Generic;

namespace LogicaNegocio
{
    public class LogicaDevolucion
    {
        private readonly DatosDevoluciones _datosDevoluciones = new DatosDevoluciones();

        // 1. LISTAR DEVOLUCIONES
        public List<Devolucion> Listar()
        {
            try
            {
                return _datosDevoluciones.Listar();
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error en la capa lógica al intentar obtener las devoluciones.", ex);
            }
        }

        // 2. CREAR DEVOLUCIÓN (Para el Cliente) - AHORA RECIBE LA RUTA DE LA IMAGEN
        public bool Crear(int idDetallePedido, string motivoCliente, string rutaImagen, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Pequeñas validaciones
                if (idDetallePedido <= 0)
                {
                    mensaje = "Debe especificar un producto válido para reportar.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(motivoCliente))
                {
                    mensaje = "El motivo del cliente es obligatorio.";
                    return false;
                }

                // Le pasamos la ruta de la imagen a la capa de datos
                bool respuesta = _datosDevoluciones.Crear(idDetallePedido, motivoCliente, rutaImagen);

                if (respuesta)
                    mensaje = "El reporte ha sido enviado al administrador correctamente.";
                else
                    mensaje = "No se pudo registrar el reporte en la base de datos.";

                return respuesta;
            }
            catch (Exception ex)
            {
                mensaje = "Ocurrió un error al crear la devolución: " + ex.Message;
                return false;
            }
        }

        public bool ProcesarReporte(int idDevolucion, string estadoReporte, string estadoDispositivo, string accionTomada, bool regresarInventario, string imeiAfectado, decimal monto)
        {
            if (idDevolucion <= 0) throw new Exception("ID de devolución inválido.");
            if (string.IsNullOrEmpty(estadoReporte)) throw new Exception("Debe seleccionar una decisión para el reporte.");

            // Si se aprueba, obligamos a que se haya llenado el formulario completo
            if (estadoReporte == "Aprobado")
            {
                if (string.IsNullOrEmpty(estadoDispositivo)) throw new Exception("Debe indicar el estado del equipo recibido.");
                if (string.IsNullOrEmpty(accionTomada)) throw new Exception("Debe seleccionar una acción a tomar.");
            }

            return _datosDevoluciones.ProcesarReporte(idDevolucion, estadoReporte, estadoDispositivo, accionTomada, regresarInventario, imeiAfectado, monto);
        }

        // 3. PROCESAR DEVOLUCIÓN (Para el Admin)
        public bool ProcesarAdmin(Devolucion devolucion, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // Validaciones para el administrador
                if (devolucion.ID_DEVOLUCION <= 0)
                {
                    mensaje = "Error: No se identificó el reporte a procesar.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(devolucion.ESTADO_REPORTE))
                {
                    mensaje = "Debe asignar un estado al reporte (Ej. Aprobado o Rechazado).";
                    return false;
                }

                bool respuesta = _datosDevoluciones.ProcesarAdmin(devolucion);

                if (respuesta)
                    mensaje = "La devolución ha sido procesada correctamente.";
                else
                    mensaje = "No se pudo procesar la devolución en la base de datos.";

                return respuesta;
            }
            catch (Exception ex)
            {
                mensaje = "Ocurrió un error al procesar la devolución: " + ex.Message;
                return false;
            }
        }
    }
}