namespace ApiREST_UTA.Domain.Entities
{
    public class RevisionDocumento
    {
        public const string Aprobado = "Aprobado";
        public const string Rechazado = "Rechazado";

        // Tipos de documento del expediente y la base de donde viene cada uno
        public static readonly string[] Tipos =
            { "TiempoRol", "Obra", "Evaluacion", "Investigacion", "Curso", "Proyecto", "TrabajoTitulacion", "Equivalencia", "Evidencia" };

        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public int DocumentoId { get; set; }
        public string Estado { get; set; } = null!;
        public string? Observacion { get; set; }
        public int RevisadoPorId { get; set; }
        public string? RevisadoPor { get; set; }
        public DateTime FechaRevision { get; set; }
    }
}
