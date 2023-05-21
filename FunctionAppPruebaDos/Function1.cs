using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace FunctionAppPruebaDos
{
    public static class Function1
    {
        [FunctionName("restaurantes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string idrestaurante = req.Query["idrestaurante"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            idrestaurante = idrestaurante ?? data?.idrestaurante;

            if (idrestaurante == null)
            {
                return new BadRequestObjectResult("Debe enviar el parametro idrestaurante " + idrestaurante);
            }
            //conectamos a Azure
            string connectionString = Environment.GetEnvironmentVariable("SqlAzure");
            string sqlSelect = "select * from restaurantes where id=" + idrestaurante;
            SqlConnection cn = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            com.Connection = cn;
            com.CommandText = sqlSelect;
            com.CommandType = CommandType.Text;

            cn.Open();

            SqlDataReader reader = com.ExecuteReader();

            string nombre = "";
            string direccion = "";
            int telefono = 0;

            if (reader.Read())
            {
                nombre = reader["nombre_restaurante"].ToString();
                direccion = reader["direccion"].ToString();
                telefono = int.Parse(reader["telefono"].ToString());
                reader.Close();
            }

            cn.Close();

            return new OkObjectResult("Estos son los datos del restaurante:| Nombre: " + nombre + " | Direccion: " + direccion + " | Telefono: " + telefono);
        }
    }
}
