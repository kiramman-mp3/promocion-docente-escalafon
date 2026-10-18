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
            var command = new SqlCommand(@"
                SELECT Id, Cedula, NombreCurso, Universidad, Facultad, FechaEvaluacion, Puntaje,
                       Horas, TipoCapacitacion, FechaInicio, FechaFin
                FROM CursosCapacitacion WHERE Cedula = @cedula
                ORDER BY COALESCE(FechaFin, FechaEvaluacion) DESC", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cursos.Add(new CursoCapacitacion
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    NombreCurso = reader["NombreCurso"] == DBNull.Value ? null : reader["NombreCurso"].ToString(),
                    Universidad = reader["Universidad"].ToString()!,
                    Facultad = reader["Facultad"].ToString()!,
                    FechaEvaluacion = Convert.ToDateTime(reader["FechaEvaluacion"]),
                    Puntaje = Convert.ToDecimal(reader["Puntaje"]),
                    Horas = reader["Horas"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Horas"]),
                    TipoCapacitacion = reader["TipoCapacitacion"].ToString()!,
                    FechaInicio = reader["FechaInicio"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = reader["FechaFin"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaFin"])
                });
            }

            return cursos;
        }
    }
}
