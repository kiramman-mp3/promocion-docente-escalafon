using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class CursoRepository : ICursoRepository
    {
        private readonly string _connectionString;

        public CursoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DEaDVConnection")!;
        }

        public async Task<IEnumerable<CursoCapacitacion>> ObtenerCursosPorCedula(string cedula)
        {
            var cursos = new List<CursoCapacitacion>();

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("SELECT Id, Cedula, Universidad, Facultad, FechaEvaluacion, Puntaje FROM CursosCapacitacion WHERE Cedula = @cedula", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cursos.Add(new CursoCapacitacion
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    Universidad = reader["Universidad"].ToString()!,
                    Facultad = reader["Facultad"].ToString()!,
                    FechaEvaluacion = Convert.ToDateTime(reader["FechaEvaluacion"]),
                    Puntaje = Convert.ToDecimal(reader["Puntaje"])
                });
            }

            return cursos;
        }
    }
}
