using System;
using System.Collections.Generic;
using System.Data;
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
                // CORRECCIÓN: SqlConnection con 'C' mayúscula
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarHistorialInventario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // Usamos el método privado para no repetir código
                            lista.Add(MapearInventario(dr));
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
                // CORRECCIÓN: SqlConnection con 'C' mayúscula
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarInventarioPorFecha", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // Usamos el método privado para no repetir código
                            lista.Add(MapearInventario(dr));
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

        // MÉTODO PRIVADO DE MAPEO
        private Inventario MapearInventario(SqlDataReader dr)
        {
            Inventario inventario = new Inventario();

            inventario.ID_INVENTARIO = Convert.ToInt32(dr["Id_Inventario"]);
            inventario.FECHA_MOVIMIENTO = Convert.ToDateTime(dr["Fecha_Movimiento"]);

            // Excelente uso de DBNull.Value
            inventario.TIPO_MOVIMIENTO = dr["Tipo_Movimiento"] != DBNull.Value ? dr["Tipo_Movimiento"].ToString() : null;
            inventario.CANTIDAD = Convert.ToInt32(dr["Cantidad"]);
            inventario.MOTIVO = dr["Motivo"] != DBNull.Value ? dr["Motivo"].ToString() : null;

            /* NOTA: Tu SP también devuelve 'Usuario_Responsable' y/o 'Producto'.
             * Si decides agregarlos a tu entidad 'Inventario' en el futuro, 
             * solo descomenta estas líneas:
             * * inventario.PRODUCTO = dr["Producto"] != DBNull.Value ? dr["Producto"].ToString() : null;
             * inventario.USUARIO_RESPONSABLE = dr["Usuario_Responsable"] != DBNull.Value ? dr["Usuario_Responsable"].ToString() : null;
             */

            return inventario;
        }
    }
}