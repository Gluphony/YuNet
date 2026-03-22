using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Cristal_Report
{
	public partial class ReportePedidos : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ReportDocument rd = new ReportDocument();

			try
			{
				string ruta = Server.MapPath("~/Reportes/RptPedidos.rpt");
				rd.Load(ruta);

				using (SqlConnection con = new SqlConnection(
					"Server=DESKTOP-H5Q9CH5\\SQLEXPRESS;Database=YuNet;Integrated Security=true"))
				{
					con.Open();

					using (SqlCommand cmd = new SqlCommand("Sp_ReportePedidos", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						SqlDataAdapter da = new SqlDataAdapter(cmd);
						DataTable dt = new DataTable();
						da.Fill(dt);

						if (dt.Rows.Count == 0)
						{
							Response.Write("No hay pedidos registrados.");
							return;
						}

						rd.SetDataSource(dt);
					}
				}

				Response.Clear();
				Response.Buffer = false;
				Response.ContentType = "application/pdf";

				rd.ExportToHttpResponse(
					ExportFormatType.PortableDocFormat,
					Response,
					false,
					"ReportePedidos"
				);

				Context.ApplicationInstance.CompleteRequest();
			}
			catch (Exception ex)
			{
				Response.Write("ERROR: " + ex.Message);
			}
			finally
			{
				rd.Close();
				rd.Dispose();
			}
		}
	}
}