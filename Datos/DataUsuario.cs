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
        public bool EditarUsuario(int idUsuario, string nombre, string correo, int idRol, bool estado, int idUsuarioLogueado) // <-- 1. Agregamos el parámetro aquí
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

                    // <-- 2. Le pasamos el "carnet" al SQL para que valide el Anti-Harakiri
                    cmd.Parameters.AddWithValue("@Id_Usuario_Logueado", idUsuarioLogueado);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (SqlException sqlEx)
                {
                    // ¡Perfecto! Esto atrapa el RAISERROR de SQL Server y lo manda a la capa de arriba
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
        public bool DesactivarUsuario(int idUsuario, int idUsuarioLogueado) // <-- 1. Agregamos el parámetro
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_DesactivarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    // <-- 2. Lo enviamos a SQL Server
                    cmd.Parameters.AddWithValue("@Id_Usuario_Logueado", idUsuarioLogueado);

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
        public bool RestablecerClave(string correo, string nuevaClave)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_RestablecerClavePorCorreo", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Correo", correo);
                    cmd.Parameters.AddWithValue("@NuevaClave", nuevaClave);

                    conexion.Open();
                    // Leemos el resultado (1 si existe, 0 si no)
                    int resultado = Convert.ToInt32(cmd.ExecuteScalar());

                    if (resultado == 1)
                        respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al restablecer clave: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}