using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class EquivalenciaRepository : IEquivalenciaRepository
    {
        private readonly string _connectionString;

        public EquivalenciaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DEaDVConnection")!;
        }

        public async Task<List<Equivalencia>> ObtenerEquivalenciasAsync(string cedula)
        {
            var equivalencias = new List<Equivalencia>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, Cedula, Tipo, Fecha, Meses, Descripcion
                FROM Equivalencias WHERE Cedula = @cedula ORDER BY Fecha DESC", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                equivalencias.Add(new Equivalencia
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    Tipo = reader["Tipo"].ToString()!,
                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                    Meses = reader["Meses"] == DBNull.Value ? null : Convert.ToInt32(reader["Meses"]),
                    Descripcion = reader["Descripcion"].ToString()!
                });
            }
            return equivalencias;
        }
    }
}
