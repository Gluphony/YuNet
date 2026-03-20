using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaUnidadExistencia
    {
        private readonly DatosUnidadExistencia _datosUnidad = new DatosUnidadExistencia();

        // 1. INSERTAR UNIDAD (Ej. Registro de un IMEI específico)
        public bool InsertarUnidadExistencia(UnidadExistencia unidad)
        {
            if (unidad == null)
                throw new ArgumentNullException(nameof(unidad), "Los datos de la unidad no pueden estar vacíos.");

            if (unidad.ID_PRODUCTO <= 0)
                throw new ArgumentException("El ID del producto al que pertenece la unidad es inválido.");

            // Sanitización del IMEI (si se proporciona)
            // Validamos que si mandan algo, no sean puros espacios
            if (!string.IsNullOrWhiteSpace(unidad.IMEI))
            {
                unidad.IMEI = unidad.IMEI.Trim();
                // Opcional: Podrías agregar una validación de longitud, ej: 
                // if (unidad.IMEI.Length != 15) throw new ArgumentException("El IMEI debe tener 15 dígitos.");
            }

            // Sanitización del Estado Físico
            if (!string.IsNullOrWhiteSpace(unidad.ESTADO_FISICO))
            {
                unidad.ESTADO_FISICO = unidad.ESTADO_FISICO.Trim().ToUpper();
            }

            return _datosUnidad.InsertarUnidadExistencia(unidad);
        }

        // 2. BUSCAR UNIDAD POR IMEI
        public DataTable BuscarUnidadPorIMEI(string imei)
        {
            if (string.IsNullOrWhiteSpace(imei))
                throw new ArgumentException("Debe proporcionar un IMEI para realizar la búsqueda.");

            return _datosUnidad.BuscarUnidadPorIMEI(imei.Trim());
        }

        // 3. LISTAR UNIDADES DISPONIBLES DE UN PRODUCTO
        public DataTable ListarUnidadesDisponibles(int idProducto)
        {
            if (idProducto <= 0)
                throw new ArgumentException("El ID del producto especificado no es válido.");

            return _datosUnidad.ListarUnidadesDisponibles(idProducto);
        }

        // 4. CAMBIAR ESTADO DE VENTA (Marcar como vendido o devuelto)
        public bool CambiarEstadoVentaUnidad(int idUnidad, bool vendido)
        {
            if (idUnidad <= 0)
                throw new ArgumentException("El ID de la unidad física no es válido.");

            return _datosUnidad.CambiarEstadoVentaUnidad(idUnidad, vendido);
        }
    }
}
