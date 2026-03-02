using Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DataAuditoria
    {
        public Conexion _con = new Conexion();
        // 1. LISTADO GENERAL (Ya lo tienes, aquí consolidado)
        public List<Auditoria> ListarAuditoriaGeneral()
        {
            var olista = new List<Auditoria>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarAuditoria", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(MapearAuditoria(reader));
                            }
                        }
                    }
                    catch (Exception ex) { /* Log error */ }
                }
            }
            return olista;
        }

        // 2. LISTAR POR USUARIO
        public List<Auditoria> ListarAuditoriaPorUsuario(int idUsuario)
        {
            var olista = new List<Auditoria>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarAuditoriaPorUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Agregamos el parámetro que pide tu SP
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(MapearAuditoria(reader));
                            }
                        }
                    }
                    catch (Exception ex) { /* Log error */ }
                }
            }
            return olista;
        }

        // 3. LISTAR POR RANGO DE FECHAS
        public List<Auditoria> ListarAuditoriaPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            var olista = new List<Auditoria>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarAuditoriaPorFecha", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Pasamos ambas fechas
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(MapearAuditoria(reader));
                            }
                        }
                    }
                    catch (Exception ex) { /* Log error */ }
                }
            }
            return olista;
        }

        // MÉTODO PRIVADO PARA EVITAR REPETIR CÓDIGO (Refactorización)
        // Este método toma el reader y crea el objeto Auditoria.
        private Auditoria MapearAuditoria(SqlDataReader reader)
        {
            return new Auditoria()
            {
                ID_AUDITORIA = Convert.ToInt64(reader["Id_Auditoria"]),
                FECHA_REGISTRO = Convert.ToDateTime(reader["Fecha_Registro"]),
                ID_USUARIO = Convert.ToInt32(reader["ID_USUARIO"]),
                ACCION = reader["ACCION"].ToString(),
                TABLA_AFECTADA = reader["TABLA_AFECTADA"].ToString(),
                DETALLE_ACTIVIDAD = reader["DETALLE_ACTIVIDAD"].ToString(),
                DIRECCION_IP = reader["DIRECCION_IP"].ToString(),
                NOMBRE_USUARIO = reader["Nombre_Usuario"].ToString()
            };
        }
    }
}