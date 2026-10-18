using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiREST_UTA.Infrastructure.Repositories
{
    public class RequisitoRepository : IRequisitoRepository
    {
        private const string Columnas = @"
            Id, GradoOrigen, GradoDestino, Normativa, Resolucion, EsPropuesta, VigenteDesde, VigenteHasta,
            ObrasTotales, ObrasEnGrado, EvaluacionMinima, EvaluacionPeriodos,
            CapacitacionHoras, CapacitacionAnios, CapacitacionPctPedagogica,
            ProyectosMeses, RequiereDireccionProyecto, TrabajosTitulacion, TitulacionNivelMinimo,
            CreadoPorId, FechaCreacion";

        private readonly string _connectionString;

        public RequisitoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DITICConnection")!;
        }

        public async Task<List<RequisitoPromocion>> ListarAsync(bool incluirHistorico)
        {
            var filtro = incluirHistorico ? "" : "WHERE VigenteHasta IS NULL";
            return await LeerVariosAsync($"SELECT {Columnas} FROM RequisitosPromocion {filtro} ORDER BY GradoOrigen, VigenteDesde DESC");
        }

        public async Task<RequisitoPromocion?> ObtenerPorIdAsync(int id)
        {
            var lista = await LeerVariosAsync($"SELECT {Columnas} FROM RequisitosPromocion WHERE Id = @id", ("@id", id));
            return lista.FirstOrDefault();
        }

        public async Task<RequisitoPromocion?> ObtenerVigenteAsync(int gradoOrigen, DateTime fecha)
        {
            var lista = await LeerVariosAsync($@"
                SELECT TOP 1 {Columnas} FROM RequisitosPromocion
                WHERE GradoOrigen = @grado AND VigenteDesde <= @fecha AND (VigenteHasta IS NULL OR VigenteHasta >= @fecha)
                ORDER BY VigenteDesde DESC",
                ("@grado", gradoOrigen), ("@fecha", fecha.Date));
            return lista.FirstOrDefault();
        }

        public async Task<RequisitoPromocion?> ObtenerAbiertoAsync(int gradoOrigen)
        {
            var lista = await LeerVariosAsync(
                $"SELECT {Columnas} FROM RequisitosPromocion WHERE GradoOrigen = @grado AND VigenteHasta IS NULL",
                ("@grado", gradoOrigen));
            return lista.FirstOrDefault();
        }

        public async Task<int> ReemplazarAsync(RequisitoPromocion nuevo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaccion = (SqlTransaction)await connection.BeginTransactionAsync();

            var cerrar = new SqlCommand(@"
                UPDATE RequisitosPromocion SET VigenteHasta = DATEADD(DAY, -1, @desde)
                WHERE GradoOrigen = @grado AND VigenteHasta IS NULL", connection, transaccion);
            cerrar.Parameters.AddWithValue("@grado", nuevo.GradoOrigen);
            cerrar.Parameters.AddWithValue("@desde", nuevo.VigenteDesde.Date);
            await cerrar.ExecuteNonQueryAsync();

            var insertar = new SqlCommand(@"
                INSERT INTO RequisitosPromocion
                    (GradoOrigen, GradoDestino, Normativa, Resolucion, EsPropuesta, VigenteDesde,
                     ObrasTotales, ObrasEnGrado, EvaluacionMinima, EvaluacionPeriodos,
                     CapacitacionHoras, CapacitacionAnios, CapacitacionPctPedagogica,
                     ProyectosMeses, RequiereDireccionProyecto, TrabajosTitulacion, TitulacionNivelMinimo, CreadoPorId)
                OUTPUT INSERTED.Id
                VALUES
                    (@origen, @destino, @normativa, @resolucion, @propuesta, @desde,
                     @obrasTotales, @obrasEnGrado, @evaluacionMinima, @evaluacionPeriodos,
                     @capHoras, @capAnios, @capPct,
                     @proyectos, @direccion, @titulacion, @nivel, @creadoPor)", connection, transaccion);
            insertar.Parameters.AddWithValue("@origen", nuevo.GradoOrigen);
            insertar.Parameters.AddWithValue("@destino", nuevo.GradoDestino);
            insertar.Parameters.AddWithValue("@normativa", nuevo.Normativa);
            insertar.Parameters.AddWithValue("@resolucion", nuevo.Resolucion);
            insertar.Parameters.AddWithValue("@propuesta", nuevo.EsPropuesta);
            insertar.Parameters.AddWithValue("@desde", nuevo.VigenteDesde.Date);
            insertar.Parameters.AddWithValue("@obrasTotales", nuevo.ObrasTotales);
            insertar.Parameters.AddWithValue("@obrasEnGrado", nuevo.ObrasEnGrado);
            insertar.Parameters.AddWithValue("@evaluacionMinima", nuevo.EvaluacionMinima);
            insertar.Parameters.AddWithValue("@evaluacionPeriodos", nuevo.EvaluacionPeriodos);
            insertar.Parameters.AddWithValue("@capHoras", nuevo.CapacitacionHoras);
            insertar.Parameters.AddWithValue("@capAnios", nuevo.CapacitacionAnios);
            insertar.Parameters.AddWithValue("@capPct", nuevo.CapacitacionPctPedagogica);
            insertar.Parameters.AddWithValue("@proyectos", nuevo.ProyectosMeses);
            insertar.Parameters.AddWithValue("@direccion", nuevo.RequiereDireccionProyecto);
            insertar.Parameters.AddWithValue("@titulacion", nuevo.TrabajosTitulacion);
            insertar.Parameters.AddWithValue("@nivel", nuevo.TitulacionNivelMinimo);
            insertar.Parameters.AddWithValue("@creadoPor", (object?)nuevo.CreadoPorId ?? DBNull.Value);
            var id = Convert.ToInt32(await insertar.ExecuteScalarAsync());

            await transaccion.CommitAsync();
            return id;
        }

        private async Task<List<RequisitoPromocion>> LeerVariosAsync(string sql, params (string Nombre, object Valor)[] parametros)
        {
            var lista = new List<RequisitoPromocion>();
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sql, connection);
            foreach (var (nombre, valor) in parametros)
                command.Parameters.AddWithValue(nombre, valor);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new RequisitoPromocion
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    GradoOrigen = Convert.ToInt32(reader["GradoOrigen"]),
                    GradoDestino = Convert.ToInt32(reader["GradoDestino"]),
                    Normativa = reader["Normativa"].ToString()!,
                    Resolucion = reader["Resolucion"].ToString()!,
                    EsPropuesta = Convert.ToBoolean(reader["EsPropuesta"]),
                    VigenteDesde = Convert.ToDateTime(reader["VigenteDesde"]),
                    VigenteHasta = reader["VigenteHasta"] == DBNull.Value ? null : Convert.ToDateTime(reader["VigenteHasta"]),
                    ObrasTotales = Convert.ToInt32(reader["ObrasTotales"]),
                    ObrasEnGrado = Convert.ToInt32(reader["ObrasEnGrado"]),
                    EvaluacionMinima = Convert.ToDecimal(reader["EvaluacionMinima"]),
                    EvaluacionPeriodos = Convert.ToInt32(reader["EvaluacionPeriodos"]),
                    CapacitacionHoras = Convert.ToInt32(reader["CapacitacionHoras"]),
                    CapacitacionAnios = Convert.ToInt32(reader["CapacitacionAnios"]),
                    CapacitacionPctPedagogica = Convert.ToDecimal(reader["CapacitacionPctPedagogica"]),
                    ProyectosMeses = Convert.ToInt32(reader["ProyectosMeses"]),
                    RequiereDireccionProyecto = Convert.ToBoolean(reader["RequiereDireccionProyecto"]),
                    TrabajosTitulacion = Convert.ToInt32(reader["TrabajosTitulacion"]),
                    TitulacionNivelMinimo = reader["TitulacionNivelMinimo"].ToString()!,
                    CreadoPorId = reader["CreadoPorId"] == DBNull.Value ? null : Convert.ToInt32(reader["CreadoPorId"]),
                    FechaCreacion = DateTime.SpecifyKind(Convert.ToDateTime(reader["FechaCreacion"]), DateTimeKind.Utc)
                });
            }
            return lista;
        }
    }
}
