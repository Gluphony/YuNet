using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaDevolucion
    {
        private readonly DatosDevoluciones _datosDevoluciones = new DatosDevoluciones();

        // 1. LISTAR DEVOLUCIONES
        public List<Devolucion> Listar()
        {
            // Como no hay parámetros de entrada, no hay mucho que validar antes de ir a la BD.
            // Simplemente pasamos la solicitud a la capa de Datos.
            try
            {
                return _datosDevoluciones.Listar();
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error en la capa lógica al intentar obtener las devoluciones.", ex);
            }
        }
        public bool Crear(Devolucion devolucion)
        {
            // Pequeñas validaciones
            if (devolucion.ID_DETALLE_DEVOLUCION <= 0)
                throw new ArgumentException("Debe especificar un detalle válido.");

            if (string.IsNullOrWhiteSpace(devolucion.MOTIVO_CLIENTE))
                throw new ArgumentException("El motivo del cliente es obligatorio.");

            return _datosDevoluciones.Crear(devolucion);
        }

    }
}