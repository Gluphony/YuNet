using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DataLogin
    {
        private readonly Conexion _con = new Conexion();

        // 1. VALIDAR LOGIN
        public DataTable ValidarLogin(string correo, string clave)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_ValidarLogin", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Pasamos los parámetros al SP
                    cmd.Parameters.AddWithValue("@Correo", correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Clave", clave ?? (object)DBNull.Value);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al validar las credenciales: " + ex.Message);
                }
            }
            return tabla; // Retornará filas si el login es exitoso, o estará vacía si falla.
        }

        // 2. CAMBIAR CONTRASEÑA
        //public bool CambiarClave(int idUsuario, string nuevaClave)
        //{
        //    bool respuesta = false;
        //    using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
        //    {
        //        try
        //        {
        //            SqlCommand cmd = new SqlCommand("Seguridad.Sp_CambiarClave", conexion);
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);
        //            cmd.Parameters.AddWithValue("@NuevaClave", nuevaClave ?? (object)DBNull.Value);

        //            conexion.Open();
        //            cmd.ExecuteNonQuery();
        //            respuesta = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Error al cambiar la contraseña: " + ex.Message);
        //        }
        //    }
        //    return respuesta;
        //}

        public bool CambiarClave(string correo, string nuevaClave)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_CambiarClave", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Correo", correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NuevaClave", nuevaClave ?? (object)DBNull.Value);

                    conexion.Open();
                    cmd.ExecuteNonQuery();

                    // Si llegó hasta aquí sin lanzar error, el SP funcionó
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    // Si el SP lanza el RAISERROR porque el correo no existe, caerá aquí
                    // Solo devolvemos false, el controlador se encarga de mostrar el mensaje genérico
                    return false;
                }
            }
            return respuesta;
        }
    }
}