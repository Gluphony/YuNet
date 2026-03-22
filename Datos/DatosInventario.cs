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

        // 1. LISTAR HISTORIAL DE INVENTARIO POR PRODUCTO (Kárdex Individual)
        public List<Inventario> ListarHistorialPorProducto(int idProducto)
        {
            List<Inventario> lista = new List<Inventario>();

            try
            {
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

        // 2. FILTRAR INVENTARIO POR RANGO DE FECHAS (Kárdex General)
        public List<Inventario> ListarPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Inventario> lista = new List<Inventario>();

            try
            {
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

        // MÉTODO PRIVADO DE MAPEO (Estandarizado para ambos SP)
        private Inventario MapearInventario(SqlDataReader dr)
        {
            Inventario inventario = new Inventario();

            // Mapeo de campos básicos
            inventario.ID_INVENTARIO = Convert.ToInt32(dr["Id_Inventario"]);
            inventario.FECHA_MOVIMIENTO = Convert.ToDateTime(dr["Fecha_Movimiento"]);
            inventario.TIPO_MOVIMIENTO = dr["Tipo_Movimiento"] != DBNull.Value ? dr["Tipo_Movimiento"].ToString() : null;
            inventario.CANTIDAD = Convert.ToInt32(dr["Cantidad"]);
            inventario.MOTIVO = dr["Motivo"] != DBNull.Value ? dr["Motivo"].ToString() : "Sin observaciones";

            // Mapeo del Modelo (Propiedad que agregamos a la Entidad)
            // IMPORTANTE: Asegúrate que en SQL el alias sea 'Modelo'
            if (ColumnExists(dr, "Modelo"))
            {
                inventario.MODELO = dr["Modelo"] != DBNull.Value ? dr["Modelo"].ToString() : "Producto Desconocido";
            }
            else if (ColumnExists(dr, "Producto")) // Por si acaso mantienes el alias 'Producto'
            {
                inventario.MODELO = dr["Producto"] != DBNull.Value ? dr["Producto"].ToString() : "Producto Desconocido";
            }

            return inventario;
        }

        // Función auxiliar para evitar errores si una columna no viene en el SP
        private bool ColumnExists(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}