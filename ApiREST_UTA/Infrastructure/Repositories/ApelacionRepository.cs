using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class ApelacionRepository : IApelacionRepository
    {
        private readonly string _connectionString;

        public ApelacionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public async Task<Apelacion?> ObtenerPorSolicitudAsync(int solicitudId)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Id, SolicitudId, Fundamentos, FechaPresentacion, PresentadaPorId, Estado,
                       ResolucionHCU, Motivacion, ResueltaPorId, FechaResolucion
                FROM Apelaciones WHERE SolicitudId = @solicitudId", connection);
            command.Parameters.AddWithValue("@solicitudId", solicitudId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new Apelacion
            {
                Id = Convert.ToInt32(reader["Id"]),
                SolicitudId = Convert.ToInt32(reader["SolicitudId"]),
                Fundamentos = reader["Fundamentos"].ToString()!,
                FechaPresentacion = DateTime.SpecifyKind(Convert.ToDateTime(reader["FechaPresentacion"]), DateTimeKind.Utc),
                PresentadaPorId = Convert.ToInt32(reader["PresentadaPorId"]),
                Estado = reader["Estado"].ToString()!,
                ResolucionHCU = reader["ResolucionHCU"] == DBNull.Value ? null : reader["ResolucionHCU"].ToString(),
                Motivacion = reader["Motivacion"] == DBNull.Value ? null : reader["Motivacion"].ToString(),
                ResueltaPorId = reader["ResueltaPorId"] == DBNull.Value ? null : Convert.ToInt32(reader["ResueltaPorId"]),
                FechaResolucion = reader["FechaResolucion"] == DBNull.Value
                    ? null
                    : DateTime.SpecifyKind(Convert.ToDateTime(reader["FechaResolucion"]), DateTimeKind.Utc)
            };
        }

        public async Task<bool> PresentarAsync(Apelacion apelacion)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaccion = (SqlTransaction)await connection.BeginTransactionAsync();

            var estado = new SqlCommand(
                "UPDATE SolicitudesPromocion SET Estado = 'Apelada' WHERE Id = @id AND Estado = 'Rechazada'", connection, transaccion);
            estado.Parameters.AddWithValue("@id", apelacion.SolicitudId);
            if (await estado.ExecuteNonQueryAsync() != 1)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            var insertar = new SqlCommand(@"
                INSERT INTO Apelaciones (SolicitudId, Fundamentos, PresentadaPorId)
                VALUES (@solicitudId, @fundamentos, @presentadaPor)", connection, transaccion);
            insertar.Parameters.AddWithValue("@solicitudId", apelacion.SolicitudId);
            insertar.Parameters.AddWithValue("@fundamentos", apelacion.Fundamentos);
            insertar.Parameters.AddWithValue("@presentadaPor", apelacion.PresentadaPorId);
            await insertar.ExecuteNonQueryAsync();

            await transaccion.CommitAsync();
            return true;
        }

        public async Task<bool> ResolverAsync(int solicitudId, string estado, string resolucionHCU, string motivacion, int resueltaPorId)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                UPDATE Apelaciones
                SET Estado = @estado, ResolucionHCU = @resolucion, Motivacion = @motivacion,
                    ResueltaPorId = @resueltaPor, FechaResolucion = SYSUTCDATETIME()
                WHERE SolicitudId = @solicitudId AND Estado = 'Pendiente'", connection);
            command.Parameters.AddWithValue("@solicitudId", solicitudId);
            command.Parameters.AddWithValue("@estado", estado);
            command.Parameters.AddWithValue("@resolucion", resolucionHCU);
            command.Parameters.AddWithValue("@motivacion", motivacion);
            command.Parameters.AddWithValue("@resueltaPor", resueltaPorId);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync() == 1;
        }
    }
}
