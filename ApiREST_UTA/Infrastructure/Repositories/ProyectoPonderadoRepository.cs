using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class ProyectoPonderadoRepository : IProyectoPonderadoRepository
    {
        private readonly string _connectionString;

        public ProyectoPonderadoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DIDEConnection")!;
        }

        // Dirección x2 y codirección x1,5 (CES art. 36 c.2)
        public async Task<int> ObtenerMesesPonderadosAsync(string cedula)
        {
            var proyectos = await ObtenerProyectosAsync(cedula);
            return (int)proyectos.Sum(p => p.MesesPonderados);
        }

        public async Task<List<ProyectoParticipacion>> ObtenerProyectosAsync(string cedula)
        {
            var proyectos = new List<ProyectoParticipacion>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, Cedula, NombreProyecto, Rol, TipoProyecto, MesesParticipacion, FechaInicio, FechaFin
                FROM ParticipacionProyecto WHERE Cedula = @cedula ORDER BY FechaInicio DESC", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                proyectos.Add(new ProyectoParticipacion
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    NombreProyecto = reader["NombreProyecto"] == DBNull.Value ? null : reader["NombreProyecto"].ToString(),
                    Rol = reader["Rol"].ToString()!,
                    TipoProyecto = reader["TipoProyecto"].ToString()!,
                    Meses = Convert.ToInt32(reader["MesesParticipacion"]),
                    FechaInicio = reader["FechaInicio"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = reader["FechaFin"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaFin"])
                });
            }
            return proyectos;
        }
    }
}
