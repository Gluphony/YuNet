using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class logicaAuditoria
    {
        // Instancia privada y de solo lectura de tu capa de datos
        private readonly DataAuditoria _dataAuditoria = new DataAuditoria();

        // 1. LISTADO GENERAL
        public List<Auditoria> ListarAuditoriaGeneral()
        {
            // Como no hay reglas complejas, simplemente pasamos la petición a Datos
            return _dataAuditoria.ListarAuditoriaGeneral();
        }

        // 2. LISTAR POR USUARIO
        public List<Auditoria> ListarAuditoriaPorUsuario(int idUsuario)
        {
            // REGLA DE NEGOCIO: El ID no puede ser 0 o negativo
            if (idUsuario <= 0)
            {
                throw new ArgumentException("El ID del usuario no es válido para la búsqueda.");
            }

            return _dataAuditoria.ListarAuditoriaPorUsuario(idUsuario);
        }

        // 3. LISTAR POR RANGO DE FECHAS
        public List<Auditoria> ListarAuditoriaPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            // REGLA DE NEGOCIO: La fecha de inicio no puede ser después de la fecha fin
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha final.");
            }

            // REGLA DE NEGOCIO (Opcional): Que no busquen auditorías en el futuro
            if (fechaInicio > DateTime.Now)
            {
                throw new ArgumentException("No se pueden buscar registros en el futuro.");
            }

            return _dataAuditoria.ListarAuditoriaPorFecha(fechaInicio, fechaFin);
        }
    }
}
