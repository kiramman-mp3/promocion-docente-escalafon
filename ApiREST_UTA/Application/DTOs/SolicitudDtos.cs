namespace ApiREST_UTA.Application.DTOs
{
    public class CrearSolicitudRequest
    {
        public string Cedula { get; set; } = string.Empty;
    }

    public class RevisarDocumentoRequest
    {
        public string TipoDocumento { get; set; } = string.Empty; // TiempoRol, Obra, Evaluacion, Investigacion, Curso
        public int DocumentoId { get; set; }
        public string Estado { get; set; } = string.Empty;        // Aprobado o Rechazado
        public string? Observacion { get; set; }                  // Obligatoria al rechazar
    }

    public class ResolverSolicitudRequest
    {
        public string Estado { get; set; } = string.Empty;        // Aprobada o Rechazada
        public string? Observacion { get; set; }                  // Obligatoria al rechazar
    }

    public class RevisionDocumentoResponse
    {
        public string TipoDocumento { get; set; } = null!;
        public int DocumentoId { get; set; }
        public string Estado { get; set; } = null!;
        public string? Observacion { get; set; }
        public string? RevisadoPor { get; set; }
        public DateTime FechaRevision { get; set; }
    }

    public class SolicitudResponse
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public int RolActualId { get; set; }
        public string RolActual { get; set; } = null!;
        public string RolObjetivo { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string? SolicitadoPor { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string? ResueltoPor { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? Observacion { get; set; }
        public List<RevisionDocumentoResponse>? Revisiones { get; set; }
    }
}
