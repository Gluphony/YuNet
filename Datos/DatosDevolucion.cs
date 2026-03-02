using System;
using System.Collections.Generic;
using System.Data;
// Usa System.Data.SqlClient o Microsoft.Data.SqlClient según tu versión de .NET
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosDevoluciones
    {
        private readonly Conexion _con = new Conexion();
        // 1. LISTAR DEVOLUCIONES
        public List<Devolucion> Listar()
        {
            List<Devolucion> lista = new List<Devolucion>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ListarDevoluciones", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Devolucion devolucion = new Devolucion();

                            // Mapeo de propiedades que existen en la entidad y devuelve el SP
                            devolucion.ID_DEVOLUCION = Convert.ToInt32(dr["Id_Devolucion"]);
                            devolucion.FECHA_DEVOLUCION = Convert.ToDateTime(dr["Fecha_Devolucion"]);
                            devolucion.ESTADO_DISPOSITIVO_RECIBIDO = dr["Estado_Dispositivo_Recibido"] != DBNull.Value ? dr["Estado_Dispositivo_Recibido"].ToString() : null;
                            devolucion.ACCION_TOMADA = dr["Accion_Tomada"] != DBNull.Value ? dr["Accion_Tomada"].ToString() : null;
                            devolucion.MOTIVO_CLIENTE = dr["Motivo_Cliente"] != DBNull.Value ? dr["Motivo_Cliente"].ToString() : null;

                            /* * el SP devuelve también estos campos:
                             * - Factura_Original
                             * - Producto
                             * - IMEI
                             * - Autorizado_Por
                             * * Si agregas estas propiedades a la clase 'Devolucion' en la capa de Entidades, 
                             * se puede mapear aquí mismo de esta manera:
                             * * devolucion.FACTURA_ORIGINAL = Convert.ToInt32(dr["Factura_Original"]);
                             * devolucion.PRODUCTO = dr["Producto"].ToString();
                             * devolucion.IMEI = dr["IMEI"].ToString();
                             * devolucion.AUTORIZADO_POR = dr["Autorizado_Por"].ToString();
                             */

                            lista.Add(devolucion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo del error
                throw new Exception("Error al listar las devoluciones: " + ex.Message);
            }

            return lista;
        }
    }
}