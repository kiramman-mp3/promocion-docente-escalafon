using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class ObrasRepository : IObraRepository
    {
        private readonly string _connectionString;

        public ObrasRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DIDEConnection")!;
        }

        // Modificar este método para aceptar 'FechaInicio' y recuperar las obras después de esa fecha
        public async Task<IEnumerable<ObraResponse>> GetObrasPorCedulaAsync(string cedula, DateTime fechaInicio)
        {
            var obras = new List<ObraResponse>();

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT Id, Cedula, FechaPublicacion, NombreRevista, Pdf FROM Obras WHERE Cedula = @cedula AND FechaPublicacion >= @fechaInicio", connection);
            command.Parameters.AddWithValue("@cedula", cedula);
            command.Parameters.AddWithValue("@fechaInicio", fechaInicio);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                obras.Add(new ObraResponse
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    FechaPublicacion = Convert.ToDateTime(reader["FechaPublicacion"]),
                    NombreRevista = reader["NombreRevista"].ToString()!,
                    PdfBase64 = Convert.ToBase64String((byte[])reader["Pdf"]!)  // Convertir el archivo PDF a Base64
                });
            }

            return obras;
        }
    }
}
