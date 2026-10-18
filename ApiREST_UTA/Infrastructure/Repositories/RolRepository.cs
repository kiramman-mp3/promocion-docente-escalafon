using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
                         RequiereCapacitacionHoras, RequiereInvestigacionMeses
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
                   RequiereCapacitacionHoras, RequiereInvestigacionMeses
            FROM Roles
            WHERE Nombre = @nombre", connection);

            command.Parameters.AddWithValue("@nombre", nombre.Trim());

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? LeerRol(reader) : null;
        }

        public async Task<List<Rol>> ListarAsync()
        {
            var roles = new List<Rol>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
            SELECT Id, Nombre, RequiereAniosEnRol, RequiereObras, RequiereEvaluacion,
                   RequiereCapacitacionHoras, RequiereInvestigacionMeses
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
                RequiereInvestigacionMeses = Convert.ToInt32(reader["RequiereInvestigacionMeses"])
            };
        }
    }
}
