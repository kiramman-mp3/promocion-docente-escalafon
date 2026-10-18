using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class InvestigacionRepository : IInvestigacionRepository
    {
        private readonly string _connectionString;

        public InvestigacionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DIDEConnection")!;
        }

        public async Task<List<Investigacion>> ObtenerInvestigacionesPorCedula(string cedula)
        {
            var investigaciones = new List<Investigacion>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT Id, Cedula, Nombre, Encargado, Horas, Pdf FROM Investigaciones WHERE Cedula = @Cedula", connection);
            command.Parameters.AddWithValue("@Cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                investigaciones.Add(new Investigacion
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString(),
                    Nombre = reader["Nombre"].ToString(),
                    Encargado = reader["Encargado"].ToString(),
                    Horas = Convert.ToInt32(reader["Horas"]),
                    Pdf = (byte[])reader["Pdf"]
                });
            }

            return investigaciones;
        }
    }
}