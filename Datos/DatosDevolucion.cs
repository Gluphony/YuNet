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

                            devolucion.ESTADO_DISPOSITIVO_RECIBIDO = dr["Estado_Dispositivo_Recibido"] != DBNull.Value ? dr["Estado_Dispositivo_Recibido"].ToString() : "";
                            devolucion.ACCION_TOMADA = dr["Accion_Tomada"] != DBNull.Value ? dr["Accion_Tomada"].ToString() : "";

                            devolucion.REGRESAR_INVENTARIO = dr["Reingreso_Stock"].ToString() == "SÍ";

                            devolucion.FACTURA_ORIGINAL = Convert.ToInt32(dr["Factura_Original"]);
                            devolucion.PRODUCTO_NOMBRE = dr["Producto"].ToString();
                            devolucion.IMEI = dr["IMEI"].ToString();
                            devolucion.AUTORIZADO_POR_NOMBRE = dr["Autorizado_Por"] != DBNull.Value ? dr["Autorizado_Por"].ToString() : "Pendiente";

                            if (HasColumn(dr, "ESTADO_REPORTE"))
                            {
                                devolucion.ESTADO_REPORTE = dr["ESTADO_REPORTE"].ToString();
                            }

                            // 👇 EXTRAEMOS LA RUTA DE LA IMAGEN SI EXISTE 👇
                            if (HasColumn(dr, "RUTA_EVIDENCIA"))
                            {
                                devolucion.RUTA_EVIDENCIA = dr["RUTA_EVIDENCIA"] != DBNull.Value ? dr["RUTA_EVIDENCIA"].ToString() : null;
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

        public bool ProcesarReporte(int idDevolucion, string estadoReporte, string estadoDispositivo, string accionTomada, bool regresarInventario, string imeiAfectado, decimal monto)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("Venta.Sp_ProcesarDevolucion", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IdDevolucion", idDevolucion);
                        cmd.Parameters.AddWithValue("@EstadoReporte", estadoReporte ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EstadoDispositivo", estadoDispositivo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AccionTomada", accionTomada ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RegresarInventario", regresarInventario);
                        cmd.Parameters.AddWithValue("@ImeiAfectado", imeiAfectado ?? "");
                        cmd.Parameters.AddWithValue("@Monto", monto);

                        conexion.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                int exito = Convert.ToInt32(dr["Exito"]);
                                if (exito == 1) return true;
                                else throw new Exception(dr["Mensaje"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return respuesta;
        }

        // 2. CREAR DEVOLUCIÓN (Para el Cliente) - AHORA RECIBE LA RUTA
        public bool Crear(int idDetallePedido, string motivoCliente, string rutaImagen)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_SolicitarDevolucionCliente", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Detalle_Pedido", idDetallePedido);
                    cmd.Parameters.AddWithValue("@Motivo_Cliente", motivoCliente);

                    // 👇 INYECTAMOS EL NUEVO PARÁMETRO DE LA RUTA 👇
                    cmd.Parameters.AddWithValue("@Ruta_Evidencia", rutaImagen ?? (object)DBNull.Value);

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
                    cmd.ExecuteNonQuery();

                    respuesta = true;
                }
            }
            catch (Exception ex)
            {
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