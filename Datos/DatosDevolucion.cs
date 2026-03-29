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

        // 1. LISTAR DEVOLUCIONES (Para el Admin)
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

                            // Estos pueden venir nulos de la BD si aún no se ha procesado
                            devolucion.ESTADO_DISPOSITIVO_RECIBIDO = dr["Estado_Dispositivo_Recibido"] != DBNull.Value ? dr["Estado_Dispositivo_Recibido"].ToString() : "";
                            devolucion.ACCION_TOMADA = dr["Accion_Tomada"] != DBNull.Value ? dr["Accion_Tomada"].ToString() : "";

                            devolucion.REGRESAR_INVENTARIO = dr["Reingreso_Stock"].ToString() == "SÍ";

                            // Mapeo de los campos calculados del SP
                            devolucion.FACTURA_ORIGINAL = Convert.ToInt32(dr["Factura_Original"]);
                            devolucion.PRODUCTO_NOMBRE = dr["Producto"].ToString();
                            devolucion.IMEI = dr["IMEI"].ToString();
                            devolucion.AUTORIZADO_POR_NOMBRE = dr["Autorizado_Por"] != DBNull.Value ? dr["Autorizado_Por"].ToString() : "Pendiente";

                            // ¡IMPORTANTE! Agregar el estado del reporte para saber si es Pendiente, Aprobado, etc.
                            // Asegúrate de que tu clase Devolucion.cs en Entidades tenga la propiedad ESTADO_REPORTE
                            if (HasColumn(dr, "ESTADO_REPORTE"))
                            {
                                devolucion.ESTADO_REPORTE = dr["ESTADO_REPORTE"].ToString();
                            }

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

        // 2. CREAR DEVOLUCIÓN (Para el Cliente)
        public bool Crear(int idDetallePedido, string motivoCliente)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    // Usamos el nuevo SP que solo pide el ID del detalle y el motivo
                    SqlCommand cmd = new SqlCommand("Venta.Sp_SolicitarDevolucionCliente", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Detalle_Pedido", idDetallePedido);
                    cmd.Parameters.AddWithValue("@Motivo_Cliente", motivoCliente);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                        respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el reporte del cliente: " + ex.Message);
            }

            return respuesta;
        }

        // 3. PROCESAR DEVOLUCIÓN (Para el Admin)
        // 3. PROCESAR DEVOLUCIÓN (Para el Admin)
        public bool ProcesarAdmin(Devolucion devolucion)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ProcesarDevolucionAdmin", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Devolucion", devolucion.ID_DEVOLUCION);
                    cmd.Parameters.AddWithValue("@Id_Usuario_Autoriza", devolucion.ID_USUARIO_AUTORIZA);
                    cmd.Parameters.AddWithValue("@Estado_Dispositivo_Recibido", devolucion.ESTADO_DISPOSITIVO_RECIBIDO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Accion_Tomada", devolucion.ACCION_TOMADA ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Regresar_Inventario", devolucion.REGRESAR_INVENTARIO);
                    cmd.Parameters.AddWithValue("@Estado_Reporte", devolucion.ESTADO_REPORTE ?? (object)DBNull.Value);

                    conexion.Open();

                    // Simplemente ejecutamos el comando. 
                    // Si el SP falla, SQL Server lanzará un error que será atrapado por el "catch" abajo.
                    cmd.ExecuteNonQuery();

                    // Si llegamos a esta línea sin errores, el SP fue un éxito.
                    respuesta = true;
                }
            }
            catch (Exception ex)
            {
                // Solo entraremos aquí si SQL lanza un error real (ej. foreign key, timeout, rollback en el SP)
                throw new Exception("Error al procesar la devolución: " + ex.Message);
            }

            return respuesta;
        }

        // Función auxiliar para verificar si una columna existe en el DataReader
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}