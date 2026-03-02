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
    public class DatosPermisos
    {
        private readonly Conexion _con = new Conexion();

        // 1. LISTAR PERMISOS
        public List<Permisos> ListarPermisos()
        {
            var olista = new List<Permisos>();

            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new Permisos()
                                {
                                    ID_PERMISO = Convert.ToInt32(reader["IdPermiso"]),
                                    CLAVE_PERMISO = reader["ClavePermiso"].ToString(),
                                    DESCRIPCION = reader["Descripcion"].ToString(),
                                    ESTADO = Convert.ToBoolean(reader["Estado"])
                                });
                            }
                        }
                    }
                    catch (Exception) { /* Log error */ }
                }
            }
            return olista;
        }

        // 2. CREAR PERMISO
        public bool CrearPermiso(Permisos oPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_Creacion_ClavePermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Nota: Tu SP solo pide Clave y Descripcion (Estado va a 1 por defecto en SQL)
                    cmd.Parameters.AddWithValue("@Clave_Permiso", oPermiso.CLAVE_PERMISO);
                    cmd.Parameters.AddWithValue("@Descripcion", oPermiso.DESCRIPCION);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }

        // 3. EDITAR PERMISO
        public bool EditarPermiso(Permisos oPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_EditarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Permiso", oPermiso.ID_PERMISO);
                    cmd.Parameters.AddWithValue("@Clave_Permiso", oPermiso.CLAVE_PERMISO);
                    cmd.Parameters.AddWithValue("@Descripcion", oPermiso.DESCRIPCION);
                    cmd.Parameters.AddWithValue("@Estado", oPermiso.ESTADO);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }

        // 4. DESACTIVAR PERMISO
        public bool DesactivarPermiso(int idPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_DesactivarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Permiso", idPermiso);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }
    }
}
