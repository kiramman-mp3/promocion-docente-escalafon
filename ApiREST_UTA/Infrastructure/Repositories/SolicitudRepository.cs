using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class SolicitudRepository : ISolicitudRepository
    {
        private const string SelectSolicitud = @"
            SELECT s.Id, s.Cedula, s.RolActualId, s.RolActual, s.RolObjetivo, s.Estado,
                   s.SolicitadoPorId, CONCAT(us.Nombre, ' ', us.Apellido) AS SolicitadoPor, s.FechaSolicitud,
                   s.ResueltoPorId, CONCAT(ur.Nombre, ' ', ur.Apellido) AS ResueltoPor, s.FechaResolucion, s.Observacion
            FROM SolicitudesPromocion s
            JOIN Usuarios us ON us.Id = s.SolicitadoPorId
            LEFT JOIN Usuarios ur ON ur.Id = s.ResueltoPorId";

        private readonly string _connectionString;

        public SolicitudRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public async Task<int> CrearAsync(SolicitudPromocion solicitud)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                INSERT INTO SolicitudesPromocion (Cedula, RolActualId, RolActual, RolObjetivo, SolicitadoPorId)
                OUTPUT INSERTED.Id
                VALUES (@cedula, @rolActualId, @rolActual, @rolObjetivo, @solicitadoPorId)", connection);
            command.Parameters.AddWithValue("@cedula", solicitud.Cedula);
            command.Parameters.AddWithValue("@rolActualId", solicitud.RolActualId);
            command.Parameters.AddWithValue("@rolActual", solicitud.RolActual);
            command.Parameters.AddWithValue("@rolObjetivo", solicitud.RolObjetivo);
            command.Parameters.AddWithValue("@solicitadoPorId", solicitud.SolicitadoPorId);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<SolicitudPromocion?> ObtenerPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(SelectSolicitud + " WHERE s.Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? LeerSolicitud(reader) : null;
        }

        public async Task<List<SolicitudPromocion>> ListarAsync(string? estado, string? cedula)
        {
            var solicitudes = new List<SolicitudPromocion>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(SelectSolicitud + @"
                WHERE (@estado IS NULL OR s.Estado = @estado)
                  AND (@cedula IS NULL OR s.Cedula = @cedula)
                ORDER BY CASE WHEN s.Estado = 'Pendiente' THEN 0 ELSE 1 END, s.FechaSolicitud DESC", connection);
            command.Parameters.AddWithValue("@estado", (object?)estado ?? DBNull.Value);
            command.Parameters.AddWithValue("@cedula", (object?)cedula ?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                solicitudes.Add(LeerSolicitud(reader));

            return solicitudes;
        }

        public async Task<bool> ExistePendienteAsync(string cedula)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(
                "SELECT COUNT(*) FROM SolicitudesPromocion WHERE Cedula = @cedula AND Estado = 'Pendiente'", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task<List<RevisionDocumento>> ObtenerRevisionesAsync(int solicitudId)
        {
            var revisiones = new List<RevisionDocumento>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT r.Id, r.SolicitudId, r.TipoDocumento, r.DocumentoId, r.Estado, r.Observacion,
                       r.RevisadoPorId, CONCAT(u.Nombre, ' ', u.Apellido) AS RevisadoPor, r.FechaRevision
                FROM RevisionesDocumento r
                JOIN Usuarios u ON u.Id = r.RevisadoPorId
                WHERE r.SolicitudId = @solicitudId", connection);
            command.Parameters.AddWithValue("@solicitudId", solicitudId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                revisiones.Add(new RevisionDocumento
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    SolicitudId = Convert.ToInt32(reader["SolicitudId"]),
                    TipoDocumento = reader["TipoDocumento"].ToString()!,
                    DocumentoId = Convert.ToInt32(reader["DocumentoId"]),
                    Estado = reader["Estado"].ToString()!,
                    Observacion = reader["Observacion"] == DBNull.Value ? null : reader["Observacion"].ToString(),
                    RevisadoPorId = Convert.ToInt32(reader["RevisadoPorId"]),
                    RevisadoPor = reader["RevisadoPor"].ToString(),
                    FechaRevision = ComoUtc(reader["FechaRevision"])
                });
            }

            return revisiones;
        }

        // Inserta o actualiza la revisión del documento (una por documento dentro de la solicitud)
        public async Task GuardarRevisionAsync(RevisionDocumento revision)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                UPDATE RevisionesDocumento
                SET Estado = @estado, Observacion = @observacion, RevisadoPorId = @revisadoPorId, FechaRevision = SYSUTCDATETIME()
                WHERE SolicitudId = @solicitudId AND TipoDocumento = @tipo AND DocumentoId = @documentoId;

                IF @@ROWCOUNT = 0
                    INSERT INTO RevisionesDocumento (SolicitudId, TipoDocumento, DocumentoId, Estado, Observacion, RevisadoPorId)
                    VALUES (@solicitudId, @tipo, @documentoId, @estado, @observacion, @revisadoPorId);", connection);
            command.Parameters.AddWithValue("@solicitudId", revision.SolicitudId);
            command.Parameters.AddWithValue("@tipo", revision.TipoDocumento);
            command.Parameters.AddWithValue("@documentoId", revision.DocumentoId);
            command.Parameters.AddWithValue("@estado", revision.Estado);
            command.Parameters.AddWithValue("@observacion", (object?)revision.Observacion ?? DBNull.Value);
            command.Parameters.AddWithValue("@revisadoPorId", revision.RevisadoPorId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> ResolverAsync(int id, string estado, string? observacion, int resueltoPorId)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                UPDATE SolicitudesPromocion
                SET Estado = @estado, Observacion = @observacion, ResueltoPorId = @resueltoPorId, FechaResolucion = SYSUTCDATETIME()
                WHERE Id = @id AND Estado = 'Pendiente'", connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@estado", estado);
            command.Parameters.AddWithValue("@observacion", (object?)observacion ?? DBNull.Value);
            command.Parameters.AddWithValue("@resueltoPorId", resueltoPorId);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync() == 1;
        }

        // Las fechas se guardan con SYSUTCDATETIME(): marcarlas como UTC para que el JSON lleve la "Z"
        private static DateTime ComoUtc(object valor) =>
            DateTime.SpecifyKind(Convert.ToDateTime(valor), DateTimeKind.Utc);

        private static SolicitudPromocion LeerSolicitud(SqlDataReader reader)
        {
            return new SolicitudPromocion
            {
                Id = Convert.ToInt32(reader["Id"]),
                Cedula = reader["Cedula"].ToString()!,
                RolActualId = Convert.ToInt32(reader["RolActualId"]),
                RolActual = reader["RolActual"].ToString()!,
                RolObjetivo = reader["RolObjetivo"].ToString()!,
                Estado = reader["Estado"].ToString()!,
                SolicitadoPorId = Convert.ToInt32(reader["SolicitadoPorId"]),
                SolicitadoPor = reader["SolicitadoPor"].ToString(),
                FechaSolicitud = ComoUtc(reader["FechaSolicitud"]),
                ResueltoPorId = reader["ResueltoPorId"] == DBNull.Value ? null : Convert.ToInt32(reader["ResueltoPorId"]),
                ResueltoPor = reader["ResueltoPorId"] == DBNull.Value ? null : reader["ResueltoPor"].ToString(),
                FechaResolucion = reader["FechaResolucion"] == DBNull.Value ? null : ComoUtc(reader["FechaResolucion"]),
                Observacion = reader["Observacion"] == DBNull.Value ? null : reader["Observacion"].ToString()
            };
        }
    }
}
