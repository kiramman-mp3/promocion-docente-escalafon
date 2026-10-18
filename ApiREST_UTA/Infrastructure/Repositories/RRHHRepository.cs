using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
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
                var fechaInicio = Convert.ToDateTime(reader["FechaInicio"]);
                return new TiempoRolResponse
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    RolActual = reader["RolActual"].ToString()!,
                    FechaInicio = fechaInicio,
                    AniosEnRol = Fechas.AniosCompletos(fechaInicio, DateTime.Today),
                    PdfBase64 = Convert.ToBase64String((byte[])reader["Pdf"])
                };
            }

            return null;
        }

        public async Task<TrayectoriaDocente?> ObtenerTrayectoriaAsync(string cedula)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Cedula, FechaIngresoInstitucion, FechaIngresoTitular, MesesNoTitular, MesesTecnicoDocente, IngresoConPhD
                FROM TrayectoriaDocente WHERE Cedula = @cedula", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new TrayectoriaDocente
            {
                Cedula = reader["Cedula"].ToString()!,
                FechaIngresoInstitucion = Convert.ToDateTime(reader["FechaIngresoInstitucion"]),
                FechaIngresoTitular = Convert.ToDateTime(reader["FechaIngresoTitular"]),
                MesesNoTitular = Convert.ToInt32(reader["MesesNoTitular"]),
                MesesTecnicoDocente = Convert.ToInt32(reader["MesesTecnicoDocente"]),
                IngresoConPhD = Convert.ToBoolean(reader["IngresoConPhD"])
            };
        }

        public async Task<List<CargoAutoridad>> ObtenerCargosAutoridadAsync(string cedula)
        {
            var cargos = new List<CargoAutoridad>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, Cedula, Cargo, TipoCargo, FechaInicio, FechaFin
                FROM CargosAutoridad WHERE Cedula = @cedula ORDER BY FechaInicio", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cargos.Add(new CargoAutoridad
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    Cargo = reader["Cargo"].ToString()!,
                    TipoCargo = reader["TipoCargo"].ToString()!,
                    FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = reader["FechaFin"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaFin"])
                });
            }
            return cargos;
        }

        public async Task<List<Sabatico>> ObtenerSabaticosAsync(string cedula)
        {
            var sabaticos = new List<Sabatico>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, Cedula, FechaInicio, FechaFin, InformeAprobado
                FROM Sabaticos WHERE Cedula = @cedula ORDER BY FechaInicio", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sabaticos.Add(new Sabatico
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Cedula = reader["Cedula"].ToString()!,
                    FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                    InformeAprobado = Convert.ToBoolean(reader["InformeAprobado"])
                });
            }
            return sabaticos;
        }
    }
}
