using System;
using System.Collections.Generic;
using System.Data;
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

                            // 1. Validamos nulos al leer de la BD para evitar "NullReferenceException"
                            devolucion.MOTIVO_CLIENTE = dr["Motivo_Cliente"] != DBNull.Value ? dr["Motivo_Cliente"].ToString() : string.Empty;
                            devolucion.ESTADO_DISPOSITIVO_RECIBIDO = dr["Estado_Dispositivo_Recibido"] != DBNull.Value ? dr["Estado_Dispositivo_Recibido"].ToString() : string.Empty;
                            devolucion.ACCION_TOMADA = dr["Accion_Tomada"] != DBNull.Value ? dr["Accion_Tomada"].ToString() : string.Empty;

                            devolucion.REGRESAR_INVENTARIO = dr["Reingreso_Stock"].ToString() == "SÍ";

                            // Mapeo de los campos calculados del SP
                            devolucion.FACTURA_ORIGINAL = dr["Factura_Original"] != DBNull.Value ? Convert.ToInt32(dr["Factura_Original"]) : 0;
                            devolucion.PRODUCTO_NOMBRE = dr["Producto"] != DBNull.Value ? dr["Producto"].ToString() : string.Empty;
                            devolucion.IMEI = dr["IMEI"] != DBNull.Value ? dr["IMEI"].ToString() : string.Empty;
                            devolucion.AUTORIZADO_POR_NOMBRE = dr["Autorizado_Por"] != DBNull.Value ? dr["Autorizado_Por"].ToString() : string.Empty;

                            lista.Add(devolucion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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

                    // Estos IDs son los que vendrán de tus Selects/Dropdowns en la vista
                    cmd.Parameters.AddWithValue("@Id_Detalle_Devolucion", devolucion.ID_DETALLE_DEVOLUCION);
                    cmd.Parameters.AddWithValue("@Id_Usuario_Autoriza", devolucion.ID_USUARIO_AUTORIZA);

                    // 2. Mejor control de nulos al insertar
                    cmd.Parameters.AddWithValue("@Motivo_Cliente", string.IsNullOrWhiteSpace(devolucion.MOTIVO_CLIENTE) ? (object)DBNull.Value : devolucion.MOTIVO_CLIENTE.Trim());
                    cmd.Parameters.AddWithValue("@Estado_Dispositivo_Recibido", string.IsNullOrWhiteSpace(devolucion.ESTADO_DISPOSITIVO_RECIBIDO) ? (object)DBNull.Value : devolucion.ESTADO_DISPOSITIVO_RECIBIDO.Trim());
                    cmd.Parameters.AddWithValue("@Accion_Tomada", string.IsNullOrWhiteSpace(devolucion.ACCION_TOMADA) ? (object)DBNull.Value : devolucion.ACCION_TOMADA.Trim());

                    cmd.Parameters.AddWithValue("@Regresar_Inventario", devolucion.REGRESAR_INVENTARIO);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                        respuesta = true;
                }
            }
            catch (SqlException sqlEx)
            {
                // 3. Atrapamos errores propios de la Base de Datos
                throw new Exception(sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error de conexión al registrar la devolución: " + ex.Message);
            }

            return respuesta;
        }
    }
}