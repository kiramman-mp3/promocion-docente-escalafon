using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class TesisRepository : ITesisRepository
    {
        private readonly string _connectionString;

        public TesisRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DACConnection")!;
        }

        public async Task<int> ObtenerTesisPorCedulaAsync(string cedula)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(
                "SELECT COUNT(*) FROM TrabajosTitulacionDirigidos WHERE Cedula = @cedula AND Nivel = N'Doctorado'", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<TrabajoTitulacion>> ObtenerTrabajosTitulacionAsync(string cedula)
        {
            var trabajos = new List<TrabajoTitulacion>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, Cedula, Titulo, Nivel, Rol, FechaAprobacion, Universidad
                FROM TrabajosTitulacionDirigidos WHERE Cedula = @cedula ORDER BY FechaAprobacion DESC", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                trabajos.Add(new TrabajoTitulacion
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    Titulo = reader["Titulo"].ToString()!,
                    Nivel = reader["Nivel"].ToString()!,
                    Rol = reader["Rol"].ToString()!,
                    FechaAprobacion = Convert.ToDateTime(reader["FechaAprobacion"]),
                    Universidad = reader["Universidad"].ToString()!
                });
            }
            return trabajos;
        }
    }
}
