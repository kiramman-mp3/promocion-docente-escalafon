using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class EvidenciaRepository : IEvidenciaRepository
    {
        private readonly string _connectionString;

        public EvidenciaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public async Task<int> CrearAsync(EvidenciaSolicitud evidencia)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                INSERT INTO EvidenciasSolicitud (SolicitudId, Tipo, Descripcion, NombreArchivo, Contenido, Hash, Tamano, SubidoPorId)
                OUTPUT INSERTED.Id
                VALUES (@solicitudId, @tipo, @descripcion, @nombre, @contenido, @hash, @tamano, @subidoPor)", connection);
            command.Parameters.AddWithValue("@solicitudId", evidencia.SolicitudId);
            command.Parameters.AddWithValue("@tipo", evidencia.Tipo);
            command.Parameters.AddWithValue("@descripcion", evidencia.Descripcion);
            command.Parameters.AddWithValue("@nombre", evidencia.NombreArchivo);
            command.Parameters.Add("@contenido", System.Data.SqlDbType.VarBinary, -1).Value = evidencia.Contenido!;
            command.Parameters.AddWithValue("@hash", evidencia.Hash);
            command.Parameters.AddWithValue("@tamano", evidencia.Tamano);
            command.Parameters.AddWithValue("@subidoPor", evidencia.SubidoPorId);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<List<EvidenciaSolicitud>> ListarAsync(int solicitudId)
        {
            var evidencias = new List<EvidenciaSolicitud>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT e.Id, e.SolicitudId, e.Tipo, e.Descripcion, e.NombreArchivo, e.Hash, e.Tamano, e.SubidoPorId,
                       CONCAT(u.Nombre, ' ', u.Apellido) AS SubidoPor, e.FechaSubida
                FROM EvidenciasSolicitud e JOIN Usuarios u ON u.Id = e.SubidoPorId
                WHERE e.SolicitudId = @solicitudId ORDER BY e.FechaSubida", connection);
            command.Parameters.AddWithValue("@solicitudId", solicitudId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                evidencias.Add(Leer(reader, conContenido: false));

            return evidencias;
        }

        public async Task<EvidenciaSolicitud?> ObtenerAsync(int solicitudId, int evidenciaId)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT e.Id, e.SolicitudId, e.Tipo, e.Descripcion, e.NombreArchivo, e.Hash, e.Tamano, e.SubidoPorId,
                       CONCAT(u.Nombre, ' ', u.Apellido) AS SubidoPor, e.FechaSubida, e.Contenido
                FROM EvidenciasSolicitud e JOIN Usuarios u ON u.Id = e.SubidoPorId
                WHERE e.SolicitudId = @solicitudId AND e.Id = @id", connection);
            command.Parameters.AddWithValue("@solicitudId", solicitudId);
            command.Parameters.AddWithValue("@id", evidenciaId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Leer(reader, conContenido: true) : null;
        }

        private static EvidenciaSolicitud Leer(SqlDataReader reader, bool conContenido) => new()
        {
            Id = Convert.ToInt32(reader["Id"]),
            SolicitudId = Convert.ToInt32(reader["SolicitudId"]),
            Tipo = reader["Tipo"].ToString()!,
            Descripcion = reader["Descripcion"].ToString()!,
            NombreArchivo = reader["NombreArchivo"].ToString()!,
            Hash = reader["Hash"].ToString()!,
            Tamano = Convert.ToInt32(reader["Tamano"]),
            SubidoPorId = Convert.ToInt32(reader["SubidoPorId"]),
            SubidoPor = reader["SubidoPor"].ToString(),
            FechaSubida = DateTime.SpecifyKind(Convert.ToDateTime(reader["FechaSubida"]), DateTimeKind.Utc),
            Contenido = conContenido ? (byte[])reader["Contenido"] : null
        };
    }
}
