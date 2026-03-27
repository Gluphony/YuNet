using Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Datos
{
    public class DataAuditoria
    {
        public Conexion _con = new Conexion();

        // 1. LISTADO GENERAL
        public List<Auditoria> ListarAuditoriaGeneral()
        {
            var olista = new List<Auditoria>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarAuditoria", connection))
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
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarAuditoriaPorUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
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
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarAuditoriaPorFecha", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
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

        // 4. REGISTRAR AUDITORÍA EN LA BASE DE DATOS
        public bool RegistrarAuditoria(Auditoria auditoria)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                // Asegúrate de que el nombre del SP y su esquema (ej. Seguridad.) coincida con tu SQL
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_InsertarAuditoria", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", auditoria.ID_USUARIO == 0 ? (object)DBNull.Value : auditoria.ID_USUARIO);
                    cmd.Parameters.AddWithValue("@Accion", auditoria.ACCION ?? "");
                    cmd.Parameters.AddWithValue("@Tabla_Afectada", auditoria.TABLA_AFECTADA ?? "");
                    cmd.Parameters.AddWithValue("@Detalle_Actividad", auditoria.DETALLE_ACTIVIDAD ?? "");
                    cmd.Parameters.AddWithValue("@Direccion_IP", auditoria.DIRECCION_IP ?? "Desconocida");

                    try
                    {
                        connection.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        if (filasAfectadas > 0) respuesta = true;
                    }
                    catch (Exception ex) { respuesta = false; }
                }
            }
            return respuesta;
        }

        // MÉTODO PRIVADO DE MAPEO
        private Auditoria MapearAuditoria(SqlDataReader reader)
        {
            return new Auditoria()
            {
                ID_AUDITORIA = Convert.ToInt64(reader["Id_Auditoria"]),
                FECHA_REGISTRO = Convert.ToDateTime(reader["Fecha_Registro"]),
                ACCION = reader["ACCION"].ToString(),
                TABLA_AFECTADA = reader["TABLA_AFECTADA"].ToString(),
                DETALLE_ACTIVIDAD = reader["DETALLE_ACTIVIDAD"].ToString(),
                DIRECCION_IP = reader["DIRECCION_IP"].ToString(),
                NOMBRE_USUARIO = reader["Nombre_Usuario"].ToString()
            };
        }
    }
}