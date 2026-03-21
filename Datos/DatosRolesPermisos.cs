using Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DatosRolesPermisos
    {
        private readonly Conexion _con = new Conexion();

        // 1. ASIGNAR PERMISO A UN ROL
        public bool AsignarPermiso(int idRol, int idPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_AsignarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);
                    cmd.Parameters.AddWithValue("@Id_Permiso", idPermiso);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception ex)
                    {
                        respuesta = false;
                    }
                }
            }
            return respuesta;
        }

        // 2. REVOCAR (ELIMINAR) PERMISO DE UN ROL
        public bool RevocarPermiso(int idRol, int idPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_RevocarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);
                    cmd.Parameters.AddWithValue("@Id_Permiso", idPermiso);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        respuesta = false;
                    }
                }
            }
            return respuesta;
        }

        // 3. LISTAR PERMISOS QUE TIENE UN ROL ESPECÍFICO
        public List<Permisos> ListarPermisosPorRol(int idRol)
        {
            var olista = new List<Permisos>();

            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarPermisosPorRol", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new Permisos()
                                {
                                    ID_PERMISO = Convert.ToInt32(reader["Id_Permiso"]),
                                    CLAVE_PERMISO = reader["Clave_Permiso"].ToString(),
                                    DESCRIPCION = reader["Descripcion"].ToString()
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return olista;
        }

        // 4. LISTAR TODOS LOS PERMISOS DISPONIBLES EN EL SISTEMA
        public List<Permisos> ListarTodosLosPermisos()
        {
            var olista = new List<Permisos>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Id_Permiso, Clave_Permiso, Descripcion FROM Seguridad.Permisos", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new Permisos()
                                {
                                    ID_PERMISO = Convert.ToInt32(reader["Id_Permiso"]),
                                    CLAVE_PERMISO = reader["Clave_Permiso"].ToString(),
                                    DESCRIPCION = reader["Descripcion"].ToString()
                                });
                            }
                        }
                    }
                    catch (Exception) { /* Log error */ }
                }
            }
            return olista;
        }

        // 5. INSERTAR UN NUEVO PERMISO (MÉTODO NUEVO AGREGADO)
        public bool InsertarPermiso(Permisos permiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                // Usamos un query directo para guardar el permiso en la tabla
                string query = "INSERT INTO Seguridad.Permisos (Clave_Permiso, Descripcion) VALUES (@Clave, @Desc)";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Clave", permiso.CLAVE_PERMISO);
                    cmd.Parameters.AddWithValue("@Desc", permiso.DESCRIPCION);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        respuesta = false;
                    }
                }
            }
            return respuesta;
        }
    }
}