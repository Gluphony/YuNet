using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Datos
{
    public class Conexion
    {
        private readonly string _cadenaSQL;

        public Conexion()
        {
            // Esta es la sintaxis estandar para cargar el appsettings.json
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Descargar los nugets especificados
                .AddJsonFile("appsettings.json")
                .Build();

            _cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        public string GetCadena()
        {
            return _cadenaSQL;
        }
    }
    }