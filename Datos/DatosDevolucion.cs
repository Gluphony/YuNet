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

                            devolucion.ID_DEVOLUCION = Convert.ToInt32(dr["Id_Devolucion"]);
                            devolucion.FECHA_DEVOLUCION = Convert.ToDateTime(dr["Fecha_Devolucion"]);
                            devolucion.MOTIVO_CLIENTE = dr["Motivo_Cliente"].ToString();
                            devolucion.ESTADO_DISPOSITIVO_RECIBIDO = dr["Estado_Dispositivo_Recibido"].ToString();
                            devolucion.ACCION_TOMADA = dr["Accion_Tomada"].ToString();
                            devolucion.REGRESAR_INVENTARIO = dr["Reingreso_Stock"].ToString() == "SÍ";

                            // Mapeo de los campos calculados del SP
                            devolucion.FACTURA_ORIGINAL = Convert.ToInt32(dr["Factura_Original"]);
                            devolucion.PRODUCTO_NOMBRE = dr["Producto"].ToString();
                            devolucion.IMEI = dr["IMEI"].ToString();
                            devolucion.AUTORIZADO_POR_NOMBRE = dr["Autorizado_Por"].ToString();
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
        // 2. CREAR DEVOLUCIÓN
        public bool Crear(Devolucion devolucion)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_CrearDevolucion", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Mapeamos exactamente las propiedades de tu entidad
                    cmd.Parameters.AddWithValue("@Id_Detalle_Devolucion", devolucion.ID_DETALLE_DEVOLUCION);
                    cmd.Parameters.AddWithValue("@Id_Usuario_Autoriza", devolucion.ID_USUARIO_AUTORIZA);
                    cmd.Parameters.AddWithValue("@Motivo_Cliente", devolucion.MOTIVO_CLIENTE ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado_Dispositivo_Recibido", devolucion.ESTADO_DISPOSITIVO_RECIBIDO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Accion_Tomada", devolucion.ACCION_TOMADA ?? (object)DBNull.Value);

                    // Manejo del Booleano (en SQL Server se traduce a bit: 1 o 0)
                    cmd.Parameters.AddWithValue("@Regresar_Inventario", devolucion.REGRESAR_INVENTARIO);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                        respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar la devolución: " + ex.Message);
            }

            return respuesta;
        }
    }
}
