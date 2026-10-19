using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class EvaluacionDocenteRepository : IEvaluacionDocenteRepository
    {
        private readonly string _connectionString;

        public EvaluacionDocenteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DEaDVConnection")!;
        }

        public async Task<List<EvaluacionDocente>> ObtenerEvaluacionesPorCedula(string cedula)
        {
            var lista = new List<EvaluacionDocente>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, Cedula, Periodo, PeriodoAcademico, Universidad, Carrera, Puntaje, EjercioDocencia, Pdf
                FROM EvaluacionesDocentes WHERE Cedula = @Cedula ORDER BY Periodo DESC", connection);
            command.Parameters.AddWithValue("@Cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new EvaluacionDocente
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString(),
                    Periodo = Convert.ToDateTime(reader["Periodo"]),
                    PeriodoAcademico = reader["PeriodoAcademico"] == DBNull.Value ? null : reader["PeriodoAcademico"].ToString(),
                    Universidad = reader["Universidad"].ToString(),
                    Carrera = reader["Carrera"].ToString(),
                    Puntaje = Convert.ToDecimal(reader["Puntaje"]),
                    EjercioDocencia = Convert.ToBoolean(reader["EjercioDocencia"]),
                    Pdf = (byte[])reader["Pdf"]
                });
            }

            return lista;
        }
    }
}
