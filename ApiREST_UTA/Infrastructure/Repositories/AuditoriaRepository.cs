using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly string _connectionString;

        public AuditoriaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public async Task RegistrarAsync(RegistroAuditoria r)
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                INSERT INTO Auditoria (UsuarioId, Usuario, Accion, Metodo, Ruta, Estado, Ip, Detalle)
                VALUES (@usuarioId, @usuario, @accion, @metodo, @ruta, @estado, @ip, @detalle)", connection);
            command.Parameters.AddWithValue("@usuarioId", (object?)r.UsuarioId ?? DBNull.Value);
            command.Parameters.AddWithValue("@usuario", (object?)Recortar(r.Usuario, 150) ?? DBNull.Value);
            command.Parameters.AddWithValue("@accion", Recortar(r.Accion, 100)!);
            command.Parameters.AddWithValue("@metodo", Recortar(r.Metodo, 10)!);
            command.Parameters.AddWithValue("@ruta", Recortar(r.Ruta, 300)!);
            command.Parameters.AddWithValue("@estado", r.Estado);
            command.Parameters.AddWithValue("@ip", (object?)Recortar(r.Ip, 45) ?? DBNull.Value);
            command.Parameters.AddWithValue("@detalle", (object?)Recortar(r.Detalle, 2000) ?? DBNull.Value);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<(List<RegistroAuditoria> Registros, int Total)> ConsultarAsync(FiltroAuditoria filtro)
        {
            const string Condiciones = @"
                WHERE (@desde IS NULL OR Fecha >= @desde)
                  AND (@hasta IS NULL OR Fecha < DATEADD(DAY, 1, @hasta))
                  AND (@usuarioId IS NULL OR UsuarioId = @usuarioId)
                  AND (@accion IS NULL OR Accion LIKE '%' + @accion + '%')";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand($@"
                SELECT COUNT(*) FROM Auditoria {Condiciones};
                SELECT Id, Fecha, UsuarioId, Usuario, Accion, Metodo, Ruta, Estado, Ip, Detalle
                FROM Auditoria {Condiciones}
                ORDER BY Fecha DESC, Id DESC
                OFFSET @saltar ROWS FETCH NEXT @tamano ROWS ONLY;", connection);
            command.Parameters.AddWithValue("@desde", (object?)filtro.Desde?.Date ?? DBNull.Value);
            command.Parameters.AddWithValue("@hasta", (object?)filtro.Hasta?.Date ?? DBNull.Value);
            command.Parameters.AddWithValue("@usuarioId", (object?)filtro.UsuarioId ?? DBNull.Value);
            command.Parameters.AddWithValue("@accion", (object?)filtro.Accion ?? DBNull.Value);
            command.Parameters.AddWithValue("@saltar", (Math.Max(filtro.Pagina, 1) - 1) * filtro.TamanoPagina);
            command.Parameters.AddWithValue("@tamano", filtro.TamanoPagina);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            var total = reader.GetInt32(0);
            await reader.NextResultAsync();

            var registros = new List<RegistroAuditoria>();
            while (await reader.ReadAsync())
            {
                registros.Add(new RegistroAuditoria
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    Fecha = DateTime.SpecifyKind(Convert.ToDateTime(reader["Fecha"]), DateTimeKind.Utc),
                    UsuarioId = reader["UsuarioId"] == DBNull.Value ? null : Convert.ToInt32(reader["UsuarioId"]),
                    Usuario = reader["Usuario"] == DBNull.Value ? null : reader["Usuario"].ToString(),
                    Accion = reader["Accion"].ToString()!,
                    Metodo = reader["Metodo"].ToString()!,
                    Ruta = reader["Ruta"].ToString()!,
                    Estado = Convert.ToInt32(reader["Estado"]),
                    Ip = reader["Ip"] == DBNull.Value ? null : reader["Ip"].ToString(),
                    Detalle = reader["Detalle"] == DBNull.Value ? null : reader["Detalle"].ToString()
                });
            }
            return (registros, total);
        }

        public async Task<ReporteUso> ReporteUsoAsync()
        {
            var reporte = new ReporteUso { GeneradoEn = DateTime.UtcNow };
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
                SELECT Estado, COUNT(*) AS Cantidad FROM SolicitudesPromocion GROUP BY Estado;

                SELECT AVG(CAST(DATEDIFF(HOUR, FechaSolicitud, FechaResolucion) AS FLOAT)) / 24.0
                FROM SolicitudesPromocion WHERE FechaResolucion IS NOT NULL;

                SELECT FORMAT(FechaSolicitud, 'yyyy-MM') AS Mes, COUNT(*) AS Cantidad
                FROM SolicitudesPromocion WHERE FechaSolicitud >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1))
                GROUP BY FORMAT(FechaSolicitud, 'yyyy-MM') ORDER BY Mes;

                SELECT SUM(CASE WHEN Activo = 1 THEN 1 ELSE 0 END), SUM(CASE WHEN Activo = 0 THEN 1 ELSE 0 END) FROM Usuarios;

                SELECT
                    SUM(CASE WHEN Accion = 'Auth.Login' AND Estado = 200 THEN 1 ELSE 0 END),
                    SUM(CASE WHEN Accion = 'Auth.Login' AND Estado <> 200 THEN 1 ELSE 0 END),
                    SUM(CASE WHEN Accion <> 'Auth.Login' THEN 1 ELSE 0 END),
                    SUM(CASE WHEN Estado >= 500 THEN 1 ELSE 0 END)
                FROM Auditoria WHERE Fecha >= DATEADD(DAY, -30, SYSUTCDATETIME());", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                reporte.SolicitudesPorEstado[reader.GetString(0)] = reader.GetInt32(1);

            await reader.NextResultAsync();
            if (await reader.ReadAsync() && !reader.IsDBNull(0))
                reporte.DiasPromedioHastaResolucion = Math.Round(reader.GetDouble(0), 1);

            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                reporte.SolicitudesPorMes.Add(new ConteoMensual { Mes = reader.GetString(0), Cantidad = reader.GetInt32(1) });

            await reader.NextResultAsync();
            if (await reader.ReadAsync())
            {
                reporte.UsuariosActivos = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                reporte.UsuariosDesactivados = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            }

            await reader.NextResultAsync();
            if (await reader.ReadAsync())
            {
                reporte.IngresosExitosos30Dias = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                reporte.IngresosFallidos30Dias = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                reporte.OperacionesEscritura30Dias = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                reporte.ErroresServidor30Dias = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
            }
            return reporte;
        }

        private static string? Recortar(string? texto, int maximo) =>
            texto == null ? null : texto.Length <= maximo ? texto : texto[..maximo];
    }
}
