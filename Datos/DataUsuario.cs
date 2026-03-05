using Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DataUsuario
    {
        private readonly Conexion _con = new Conexion();

        // Añade estos métodos dentro de tu clase DatosSeguridad

        // 3. LISTAR USUARIOS
        public DataTable ListarUsuarios()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar los usuarios: " + ex.Message);
                }
            }
            return tabla;
        }

        // 4. INSERTAR USUARIO
        public bool InsertarUsuario(string nombre, string correo, string clave, int idRol)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_InsertarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Clave", clave ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (SqlException sqlEx)
                {
                    // Atrapamos el RAISERROR de SQL Server (ej. "El correo ya está registrado")
                    throw new Exception(sqlEx.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error de conexión al insertar usuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        // 5. EDITAR USUARIO
        public bool EditarUsuario(int idUsuario, string nombre, string correo, int idRol, bool estado)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_EditarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);
                    cmd.Parameters.AddWithValue("@Estado", estado);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (SqlException sqlEx)
                {
                    // Atrapamos el RAISERROR de SQL Server
                    throw new Exception(sqlEx.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar usuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        // 6. DESACTIVAR USUARIO (Borrado Lógico)
        public bool DesactivarUsuario(int idUsuario)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_DesactivarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al desactivar el usuario: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}