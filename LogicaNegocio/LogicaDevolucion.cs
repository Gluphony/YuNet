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

        // 2. CREAR DEVOLUCIÓN
        public bool Crear(Devolucion devolucion)
        {
            // 1. Validar que el objeto principal no sea nulo (evita caídas abruptas del sistema)
            if (devolucion == null)
                throw new ArgumentNullException(nameof(devolucion), "Los datos de la devolución no pueden estar vacíos.");

            // 2. Validaciones Numéricas (Previenen errores de Llaves Foráneas en SQL Server)
            if (devolucion.ID_DETALLE_DEVOLUCION <= 0)
                throw new ArgumentException("Debe seleccionar un producto/detalle de venta válido.");

            if (devolucion.ID_USUARIO_AUTORIZA <= 0)
                throw new ArgumentException("Debe especificar qué usuario está autorizando esta devolución.");

            // 3. Validaciones de Texto
            if (string.IsNullOrWhiteSpace(devolucion.MOTIVO_CLIENTE))
                throw new ArgumentException("El motivo del cliente es obligatorio.");

            // 4. Sanitización (quitamos espacios accidentales al inicio y al final)
            devolucion.MOTIVO_CLIENTE = devolucion.MOTIVO_CLIENTE.Trim();

            if (!string.IsNullOrWhiteSpace(devolucion.ESTADO_DISPOSITIVO_RECIBIDO))
                devolucion.ESTADO_DISPOSITIVO_RECIBIDO = devolucion.ESTADO_DISPOSITIVO_RECIBIDO.Trim();

            if (!string.IsNullOrWhiteSpace(devolucion.ACCION_TOMADA))
                devolucion.ACCION_TOMADA = devolucion.ACCION_TOMADA.Trim();

            // 5. Si todo está perfecto, mandamos a guardar a la capa de Datos
            return _datosDevoluciones.Crear(devolucion);
        }
    }
}