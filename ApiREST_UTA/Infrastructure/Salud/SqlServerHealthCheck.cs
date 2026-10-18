using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiREST_UTA.Infrastructure.Salud
{
    // Disponibilidad de las cinco bases (Reglamento DITIC art. 2: disponibilidad de la información)
    public class SqlServerHealthCheck : IHealthCheck
    {
        private static readonly string[] Conexiones = { "DITICConnection", "RRHHConnection", "DIDEConnection", "DEaDVConnection", "DACConnection" };
        private readonly IConfiguration _configuration;

        public SqlServerHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var datos = new Dictionary<string, object>();
            var fallas = new List<string>();
            foreach (var nombre in Conexiones)
            {
                var baseDeDatos = nombre.Replace("Connection", "");
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString(nombre));
                    await connection.OpenAsync(cancellationToken);
                    using var command = new SqlCommand("SELECT 1", connection) { CommandTimeout = 5 };
                    await command.ExecuteScalarAsync(cancellationToken);
                    datos[baseDeDatos] = "disponible";
                }
                catch (Exception ex)
                {
                    datos[baseDeDatos] = "sin conexión";
                    fallas.Add($"{baseDeDatos}: {ex.GetType().Name}");
                }
            }

            return fallas.Count == 0
                ? HealthCheckResult.Healthy("Las cinco bases responden.", datos)
                : HealthCheckResult.Unhealthy(string.Join("; ", fallas), data: datos);
        }
    }
}
