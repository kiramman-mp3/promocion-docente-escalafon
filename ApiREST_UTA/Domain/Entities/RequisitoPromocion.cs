namespace ApiREST_UTA.Domain.Entities
{
    // Requisitos para promover desde GradoOrigen al grado siguiente (DITIC.dbo.RequisitosPromocion).
    // Base legal: CES RPC-SE-19-No.055-2021 arts. 76-77 y UTA 0153-HCU-2026 art. 48.
    public class RequisitoPromocion
    {
        public const string NivelGrado = "Grado";
        public const string NivelMaestria = "Maestria";
        public const string NivelDoctorado = "Doctorado";
        public static readonly string[] NivelesTitulacion = { NivelGrado, NivelMaestria, NivelDoctorado };

        public int Id { get; set; }
        public int GradoOrigen { get; set; }
        public int GradoDestino { get; set; }
        public string Normativa { get; set; } = null!;
        public string Resolucion { get; set; } = null!;
        public bool EsPropuesta { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime? VigenteHasta { get; set; }

        // Art. 76 b: obras de relevancia totales y publicadas durante el grado actual
        public int ObrasTotales { get; set; }
        public int ObrasEnGrado { get; set; }

        // Art. 76 c: promedio mínimo en las últimas N evaluaciones en las que ejerció la docencia
        public decimal EvaluacionMinima { get; set; }
        public int EvaluacionPeriodos { get; set; }

        // Art. 76 d: horas de actualización en los últimos N años y porcentaje pedagógico
        public int CapacitacionHoras { get; set; }
        public int CapacitacionAnios { get; set; }
        public decimal CapacitacionPctPedagogica { get; set; }

        // UTA art. 48 e: requisitos opcionales de la universidad
        public int ProyectosMeses { get; set; }
        public bool RequiereDireccionProyecto { get; set; }
        public int TrabajosTitulacion { get; set; }
        public string TitulacionNivelMinimo { get; set; } = NivelGrado;

        public int? CreadoPorId { get; set; }
        public DateTime FechaCreacion { get; set; }

        public bool VigenteEn(DateTime fecha) =>
            VigenteDesde.Date <= fecha.Date && (VigenteHasta == null || VigenteHasta.Value.Date >= fecha.Date);
    }
}
