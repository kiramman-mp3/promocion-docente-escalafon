using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class ComisionRepository : IComisionRepository
    {
        private const string SelectMiembro = @"
            SELECT m.Id, m.UsuarioId, CONCAT(u.Nombre, ' ', u.Apellido) AS Nombre, u.Correo, u.Cedula,
                   m.Cargo, m.EsPresidente, m.VigenteDesde, m.VigenteHasta
            FROM ComisionMiembros m JOIN Usuarios u ON u.Id = m.UsuarioId";

        private readonly string _connectionString;

        public ComisionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public async Task<List<MiembroComision>> ListarMiembrosAsync(bool incluirHistorico)
        {
            var filtro = incluirHistorico ? "" : " WHERE m.VigenteHasta IS NULL";
            return await LeerMiembrosAsync(SelectMiembro + filtro + " ORDER BY m.VigenteHasta, m.EsPresidente DESC, m.Cargo");
        }

        public async Task<MiembroComision?> ObtenerMiembroActivoPorUsuarioAsync(int usuarioId) =>
            (await LeerMiembrosAsync(SelectMiembro + " WHERE m.UsuarioId = @usuarioId AND m.VigenteHasta IS NULL",
                ("@usuarioId", usuarioId))).FirstOrDefault();

        public async Task<MiembroComision?> ObtenerMiembroAsync(int id) =>
            (await LeerMiembrosAsync(SelectMiembro + " WHERE m.Id = @id", ("@id", id))).FirstOrDefault();

        public async Task<int> AgregarMiembroAsync(MiembroComision miembro)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                INSERT INTO ComisionMiembros (UsuarioId, Cargo, EsPresidente, VigenteDesde)
                OUTPUT INSERTED.Id VALUES (@usuarioId, @cargo, @presidente, @desde)", connection);
            command.Parameters.AddWithValue("@usuarioId", miembro.UsuarioId);
            command.Parameters.AddWithValue("@cargo", miembro.Cargo);
            command.Parameters.AddWithValue("@presidente", miembro.EsPresidente);
            command.Parameters.AddWithValue("@desde", miembro.VigenteDesde.Date);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task FinalizarMiembroAsync(int id, DateTime hasta)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(
                "UPDATE ComisionMiembros SET VigenteHasta = @hasta WHERE Id = @id AND VigenteHasta IS NULL", connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@hasta", hasta.Date);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<VotoComision>> ObtenerVotosAsync(int solicitudId)
        {
            var votos = new List<VotoComision>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT v.Id, v.SolicitudId, v.MiembroId, CONCAT(u.Nombre, ' ', u.Apellido) AS Miembro, m.Cargo, m.EsPresidente,
                       v.Voto, v.Motivacion, v.CertificaPresupuesto, v.Fecha
                FROM VotosComision v
                JOIN ComisionMiembros m ON m.Id = v.MiembroId
                JOIN Usuarios u ON u.Id = m.UsuarioId
                WHERE v.SolicitudId = @solicitudId ORDER BY v.Fecha", connection);
            command.Parameters.AddWithValue("@solicitudId", solicitudId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                votos.Add(new VotoComision
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    SolicitudId = Convert.ToInt32(reader["SolicitudId"]),
                    MiembroId = Convert.ToInt32(reader["MiembroId"]),
                    Miembro = reader["Miembro"].ToString(),
                    Cargo = reader["Cargo"].ToString(),
                    EsPresidente = Convert.ToBoolean(reader["EsPresidente"]),
                    Voto = reader["Voto"].ToString()!,
                    Motivacion = reader["Motivacion"].ToString()!,
                    CertificaPresupuesto = reader["CertificaPresupuesto"] == DBNull.Value ? null : Convert.ToBoolean(reader["CertificaPresupuesto"]),
                    Fecha = DateTime.SpecifyKind(Convert.ToDateTime(reader["Fecha"]), DateTimeKind.Utc)
                });
            }
            return votos;
        }

        public async Task GuardarVotoAsync(VotoComision voto)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                UPDATE VotosComision
                SET Voto = @voto, Motivacion = @motivacion, CertificaPresupuesto = @certifica, Fecha = SYSUTCDATETIME()
                WHERE SolicitudId = @solicitudId AND MiembroId = @miembroId;

                IF @@ROWCOUNT = 0
                    INSERT INTO VotosComision (SolicitudId, MiembroId, Voto, Motivacion, CertificaPresupuesto)
                    VALUES (@solicitudId, @miembroId, @voto, @motivacion, @certifica);", connection);
            command.Parameters.AddWithValue("@solicitudId", voto.SolicitudId);
            command.Parameters.AddWithValue("@miembroId", voto.MiembroId);
            command.Parameters.AddWithValue("@voto", voto.Voto);
            command.Parameters.AddWithValue("@motivacion", voto.Motivacion);
            command.Parameters.AddWithValue("@certifica", (object?)voto.CertificaPresupuesto ?? DBNull.Value);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private async Task<List<MiembroComision>> LeerMiembrosAsync(string sql, params (string Nombre, object Valor)[] parametros)
        {
            var miembros = new List<MiembroComision>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sql, connection);
            foreach (var (nombre, valor) in parametros)
                command.Parameters.AddWithValue(nombre, valor);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                miembros.Add(new MiembroComision
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                    Nombre = reader["Nombre"].ToString(),
                    Correo = reader["Correo"].ToString(),
                    Cedula = reader["Cedula"] == DBNull.Value ? null : reader["Cedula"].ToString(),
                    Cargo = reader["Cargo"].ToString()!,
                    EsPresidente = Convert.ToBoolean(reader["EsPresidente"]),
                    VigenteDesde = Convert.ToDateTime(reader["VigenteDesde"]),
                    VigenteHasta = reader["VigenteHasta"] == DBNull.Value ? null : Convert.ToDateTime(reader["VigenteHasta"])
                });
            }
            return miembros;
        }
    }
}
