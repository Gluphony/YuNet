using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Cristal_report
{
	public partial class Reporte : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ReportDocument rd = new ReportDocument();

			try
			{
				//  Ruta del reporte
				string ruta = Server.MapPath("~/Reportes/RptInventario.rpt");
				rd.Load(ruta);

				DataTable dt = new DataTable();

				//  Conexión a BD
				using (SqlConnection con = new SqlConnection(
                    "Data Source=MSI\\SQLEXPRESS;Initial Catalog=YuNet;User ID=Marcelo;Password=Pipizza21;Trust Server Certificate=True"))
				{
					con.Open();

					using (SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarInventarioPorFecha", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						//  Parámetros
						cmd.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = DateTime.Now.AddMonths(-1);
						cmd.Parameters.Add("@FechaFin", SqlDbType.Date).Value = DateTime.Now;

						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(dt);
					}
				}

				//  Validación
				if (dt.Rows.Count == 0)
				{
					Response.Write("No hay datos para el rango de fechas seleccionado.");
					return;
				}

				//  Asignar datos al reporte
				rd.SetDataSource(dt);
				rd.Refresh();

				//  EXPORTACIÓN SEGURA (SIN ERROR COM)
				using (Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat))
				{
					byte[] buffer = new byte[stream.Length];
					stream.Read(buffer, 0, buffer.Length);

					Response.Clear();
					Response.ContentType = "application/pdf";
					Response.AddHeader("Content-Disposition", "inline; filename=ReporteInventario.pdf");
					Response.BinaryWrite(buffer);
					Response.Flush();
				}
			}
			catch (Exception ex)
			{
				Response.Write("ERROR: " + ex.Message);
			}
			finally
			{
				// Liberar recursos correctamente
				rd.Close();
				rd.Dispose();
			}
		}
	}
}