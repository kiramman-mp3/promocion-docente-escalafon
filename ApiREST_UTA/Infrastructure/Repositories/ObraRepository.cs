using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class ObrasRepository : IObraRepository
    {
        private const string Columnas = @"
            Id, Cedula, Titulo, FechaPublicacion, NombreRevista, Idioma, FiliacionUTA, TipoObra,
            ISSN, ISBN, BaseIndexacion, RevisionPares, RegistroSENADI, ValoracionExterna";

        private readonly string _connectionString;

        public ObrasRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DIDEConnection")!;
        }

        public async Task<IEnumerable<ObraResponse>> GetObrasPorCedulaAsync(string cedula, DateTime fechaInicio)
        {
            var obras = new List<ObraResponse>();

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand($@"
                SELECT {Columnas}, Pdf FROM Obras
                WHERE Cedula = @cedula AND FechaPublicacion >= @fechaInicio
                ORDER BY FechaPublicacion DESC", connection);
            command.Parameters.AddWithValue("@cedula", cedula);
            command.Parameters.AddWithValue("@fechaInicio", fechaInicio < new DateTime(1900, 1, 1) ? new DateTime(1900, 1, 1) : fechaInicio);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var obra = LeerObra(reader);
                var (relevante, motivo) = obra.EvaluarRelevancia();
                obras.Add(new ObraResponse
                {
                    Id = obra.Id,
                    Cedula = obra.Cedula,
                    Titulo = obra.Titulo,
                    FechaPublicacion = obra.FechaPublicacion,
                    NombreRevista = obra.NombreRevista,
                    Idioma = obra.Idioma,
                    TipoObra = obra.TipoObra,
                    ISSN = obra.ISSN,
                    ISBN = obra.ISBN,
                    BaseIndexacion = obra.BaseIndexacion,
                    RevisionPares = obra.RevisionPares,
                    RegistroSENADI = obra.RegistroSENADI,
                    EsRelevante = relevante,
                    MotivoNoRelevante = motivo,
                    PdfBase64 = Convert.ToBase64String((byte[])reader["Pdf"])
                });
            }

            return obras;
        }

        public async Task<List<Obra>> ObtenerObrasAsync(string cedula)
        {
            var obras = new List<Obra>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand($"SELECT {Columnas} FROM Obras WHERE Cedula = @cedula", connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                obras.Add(LeerObra(reader));

            return obras;
        }

        private static Obra LeerObra(SqlDataReader reader)
        {
            string? Texto(string columna) => reader[columna] == DBNull.Value ? null : reader[columna].ToString();

            return new Obra
            {
                Id = Convert.ToInt32(reader["Id"]),
                Cedula = reader["Cedula"].ToString()!,
                Titulo = Texto("Titulo"),
                FechaPublicacion = Convert.ToDateTime(reader["FechaPublicacion"]),
                NombreRevista = reader["NombreRevista"].ToString()!,
                Idioma = reader["Idioma"].ToString()!,
                FiliacionUTA = Convert.ToBoolean(reader["FiliacionUTA"]),
                TipoObra = reader["TipoObra"].ToString()!,
                ISSN = Texto("ISSN"),
                ISBN = Texto("ISBN"),
                BaseIndexacion = Texto("BaseIndexacion"),
                RevisionPares = Convert.ToBoolean(reader["RevisionPares"]),
                RegistroSENADI = Texto("RegistroSENADI"),
                ValoracionExterna = Convert.ToBoolean(reader["ValoracionExterna"])
            };
        }
    }
}
