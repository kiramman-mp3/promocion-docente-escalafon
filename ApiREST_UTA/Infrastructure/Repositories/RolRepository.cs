using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly string _connectionString;

        public RolRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RRHHConnection")!;
        }

        public async Task<Rol> ObtenerSiguienteRol(int rolActualId)
        {
            return await BuscarSiguienteRol(rolActualId) ?? throw new Exception("No hay rol siguiente disponible");
        }

        public async Task<Rol?> BuscarSiguienteRol(int rolActualId)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
            SELECT TOP 1 Id, Nombre, RequiereAniosEnRol, RequiereObras, RequiereEvaluacion, 
                         RequiereCapacitacionHoras, RequiereInvestigacionMeses,
                   Categoria, Nivel, Grado, PermanenciaAnios, Promocionable
            FROM Roles
            WHERE Id > @rolActualId
            ORDER BY Id ASC", connection);

            command.Parameters.AddWithValue("@rolActualId", rolActualId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return LeerRol(reader);
        }

        public async Task<Rol?> ObtenerPorNombre(string nombre)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
            SELECT Id, Nombre, RequiereAniosEnRol, RequiereObras, RequiereEvaluacion,
                   RequiereCapacitacionHoras, RequiereInvestigacionMeses,
                   Categoria, Nivel, Grado, PermanenciaAnios, Promocionable
            FROM Roles
            WHERE Nombre = @nombre", connection);

            command.Parameters.AddWithValue("@nombre", nombre.Trim());

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? LeerRol(reader) : null;
        }

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            return (await ListarAsync()).FirstOrDefault(r => r.Id == id);
        }

        public async Task<List<Rol>> ListarAsync()
        {
            var roles = new List<Rol>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
            SELECT Id, Nombre, RequiereAniosEnRol, RequiereObras, RequiereEvaluacion,
                   RequiereCapacitacionHoras, RequiereInvestigacionMeses,
                   Categoria, Nivel, Grado, PermanenciaAnios, Promocionable
            FROM Roles
            ORDER BY Id ASC", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                roles.Add(LeerRol(reader));

            return roles;
        }

        private static Rol LeerRol(SqlDataReader reader)
        {
            return new Rol
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"].ToString()!,
                RequiereAniosEnRol = Convert.ToInt32(reader["RequiereAniosEnRol"]),
                RequiereObras = Convert.ToInt32(reader["RequiereObras"]),
                RequiereEvaluacion = Convert.ToDecimal(reader["RequiereEvaluacion"]),
                RequiereCapacitacionHoras = Convert.ToInt32(reader["RequiereCapacitacionHoras"]),
                RequiereInvestigacionMeses = Convert.ToInt32(reader["RequiereInvestigacionMeses"]),
                Categoria = reader["Categoria"] == DBNull.Value ? null : reader["Categoria"].ToString(),
                Nivel = reader["Nivel"] == DBNull.Value ? null : Convert.ToInt32(reader["Nivel"]),
                Grado = reader["Grado"] == DBNull.Value ? null : Convert.ToInt32(reader["Grado"]),
                PermanenciaAnios = reader["PermanenciaAnios"] == DBNull.Value ? null : Convert.ToInt32(reader["PermanenciaAnios"]),
                Promocionable = Convert.ToBoolean(reader["Promocionable"])
            };
        }
    }
}
