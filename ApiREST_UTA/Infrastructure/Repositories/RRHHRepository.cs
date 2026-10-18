using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Interfaces.ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class RRHHRepository : IRRHHRepository
    {
        private readonly string _connectionString;

        public RRHHRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RRHHConnection")!;
        }

        public async Task<TiempoRolResponse?> ObtenerTiempoRolPorCedulaAsync(string cedula)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT Id, Cedula, RolActual, FechaInicio, Pdf FROM TiempoRol WHERE Cedula = @cedula", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new TiempoRolResponse
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    RolActual = reader["RolActual"].ToString()!,
                    FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                    AniosEnRol = DateTime.Now.Year - Convert.ToDateTime(reader["FechaInicio"]).Year,
                    PdfBase64 = Convert.ToBase64String((byte[])reader["Pdf"])
                };
            }

            return null;
        }
    }
}
