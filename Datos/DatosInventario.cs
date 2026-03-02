using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosInventario
    {
        private readonly Conexion _con = new Conexion();

        // 1. LISTAR HISTORIAL DE INVENTARIO POR PRODUCTO
        public List<Inventario> ListarHistorialPorProducto(int idProducto)
        {
            List<Inventario> lista = new List<Inventario>();

            try
            {
                using (Sqlconnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarHistorialInventario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Enviamos el parámetro que requiere el SP
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Inventario inventario = new Inventario();

                            // Mapeamos los campos que coinciden entre tu SP y tu Entidad
                            inventario.ID_INVENTARIO = Convert.ToInt32(dr["Id_Inventario"]);
                            inventario.FECHA_MOVIMIENTO = Convert.ToDateTime(dr["Fecha_Movimiento"]);
                            inventario.TIPO_MOVIMIENTO = dr["Tipo_Movimiento"] != DBNull.Value ? dr["Tipo_Movimiento"].ToString() : null;
                            inventario.CANTIDAD = Convert.ToInt32(dr["Cantidad"]);
                            inventario.MOTIVO = dr["Motivo"] != DBNull.Value ? dr["Motivo"].ToString() : null;

                            /* NOTA: Tu SP también devuelve:
                             * - Usuario_Responsable
                             * * Si agregas 'public string? USUARIO_RESPONSABLE { get; set; }' a tu entidad, puedes hacer:
                             * inventario.USUARIO_RESPONSABLE = dr["Usuario_Responsable"].ToString();
                             */

                            lista.Add(inventario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar el historial del producto: " + ex.Message);
            }

            return lista;
        }

        // 2. FILTRAR INVENTARIO POR RANGO DE FECHAS
        public List<Inventario> ListarPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Inventario> lista = new List<Inventario>();

            try
            {
                using (Sqlconnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarInventarioPorFecha", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // C# maneja automáticamente la conversión de DateTime a DATE de SQL Server
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Inventario inventario = new Inventario();

                            inventario.ID_INVENTARIO = Convert.ToInt32(dr["Id_Inventario"]);
                            inventario.FECHA_MOVIMIENTO = Convert.ToDateTime(dr["Fecha_Movimiento"]);
                            inventario.TIPO_MOVIMIENTO = dr["Tipo_Movimiento"] != DBNull.Value ? dr["Tipo_Movimiento"].ToString() : null;
                            inventario.CANTIDAD = Convert.ToInt32(dr["Cantidad"]);
                            inventario.MOTIVO = dr["Motivo"] != DBNull.Value ? dr["Motivo"].ToString() : null;

                            /* NOTA: Este SP también devuelve:
                             * - Usuario_Responsable
                             * - Producto (El nombre del producto)
                             * * Si agregas esas propiedades a tu entidad 'Inventario', aquí iría:
                             * inventario.PRODUCTO = dr["Producto"].ToString();
                             * inventario.USUARIO_RESPONSABLE = dr["Usuario_Responsable"].ToString();
                             */

                            lista.Add(inventario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al filtrar el inventario por fechas: " + ex.Message);
            }

            return lista;
        }
    }
}