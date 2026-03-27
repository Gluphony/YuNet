using Datos;
using Entidades;
using System;
using System.Collections.Generic;

namespace LogicaNegocio
{
    public class logicaAuditoria
    {
        private readonly DataAuditoria _dataAuditoria = new DataAuditoria();

        // 1. LISTADO GENERAL
        public List<Auditoria> ListarAuditoriaGeneral()
        {
            return _dataAuditoria.ListarAuditoriaGeneral();
        }

        // 2. LISTAR POR USUARIO
        public List<Auditoria> ListarAuditoriaPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException("El ID del usuario no es válido.");
            }
            return _dataAuditoria.ListarAuditoriaPorUsuario(idUsuario);
        }

        // 3. LISTAR POR RANGO DE FECHAS
        public List<Auditoria> ListarAuditoriaPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha final.");
            }
            if (fechaInicio > DateTime.Now)
            {
                throw new ArgumentException("No se pueden buscar registros en el futuro.");
            }
            return _dataAuditoria.ListarAuditoriaPorFecha(fechaInicio, fechaFin);
        }

        // 4. REGISTRAR AUDITORÍA
        public bool RegistrarAuditoria(int idUsuario, string accion, string tabla, string detalle, string ip)
        {
            // Validaciones básicas de seguridad
            if (string.IsNullOrEmpty(accion) || string.IsNullOrEmpty(tabla))
            {
                return false; // No guardamos registros vacíos o corruptos
            }

            Auditoria nuevoRegistro = new Auditoria
            {
                ID_USUARIO = idUsuario, // Enviamos el entero
                ACCION = accion,
                TABLA_AFECTADA = tabla,
                DETALLE_ACTIVIDAD = detalle,
                DIRECCION_IP = ip
            };

            return _dataAuditoria.RegistrarAuditoria(nuevoRegistro);
        }
    }
}