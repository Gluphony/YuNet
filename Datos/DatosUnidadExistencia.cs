using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosUnidadExistencia
    {

        private readonly Conexion _con = new Conexion();

        public bool InsertarUnidadExistencia(UnidadExistencia unidad)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_InsertarUnidadExistencia", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Producto", unidad.ID_PRODUCTO);
                    cmd.Parameters.AddWithValue("@IMEI", unidad.IMEI ?? (object)DBNull.Value);

                    // Si el estado físico viene nulo, pasamos DBNull para que SQL use el valor por defecto ('NUEVO')
                    cmd.Parameters.AddWithValue("@Estado_Fisico", unidad.ESTADO_FISICO ?? (object)DBNull.Value);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al insertar la unidad/IMEI: " + ex.Message);
                }
            }
            return respuesta;
        }

        public DataTable BuscarUnidadPorIMEI(string imei)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_BuscarUnidadPorIMEI", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IMEI", imei);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al buscar la unidad por IMEI: " + ex.Message);
                }
            }
            return tabla;
        }

        public DataTable ListarUnidadesDisponibles(int idProducto)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarUnidadesDisponibles", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar las unidades disponibles: " + ex.Message);
                }
            }
            return tabla;
        }

        public bool CambiarEstadoVentaUnidad(int idUnidad, bool vendido)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_CambiarEstadoVentaUnidad", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Unidad", idUnidad);
                    cmd.Parameters.AddWithValue("@Vendido", vendido);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al cambiar el estado de venta de la unidad: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}