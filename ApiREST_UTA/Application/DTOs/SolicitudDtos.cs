using ApiREST_UTA.Application.Services.Promocion;

namespace ApiREST_UTA.Application.DTOs
{
    public class CrearSolicitudRequest
    {
        public string Cedula { get; set; } = string.Empty;
    }

    public class RevisarDocumentoRequest
    {
        public string TipoDocumento { get; set; } = string.Empty; // TiempoRol, Obra, Evaluacion, Curso, Investigacion, Proyecto, TrabajoTitulacion, Equivalencia, Evidencia
        public int DocumentoId { get; set; }
        public string Estado { get; set; } = string.Empty;        // Aprobado o Rechazado
        public string? Observacion { get; set; }                  // Obligatoria al rechazar
    }

    public class VotoRequest
    {
        public string Voto { get; set; } = string.Empty;          // AFavor, EnContra o Abstencion
        public string Motivacion { get; set; } = string.Empty;    // Obligatoria
        public bool? CertificaPresupuesto { get; set; }           // Solo el Director Financiero
    }

    public class CerrarVotacionRequest
    {
        public string Motivacion { get; set; } = string.Empty;    // Motivación de la resolución (obligatoria)
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

    // Documento del expediente que Talento Humano debe revisar antes de enviar a la Comisión
    public class DocumentoSolicitudResponse
    {
        public string Tipo { get; set; } = null!;
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
        public RevisionDocumentoResponse? Revision { get; set; }
    }

    public class VotoResponse
    {
        public string? Miembro { get; set; }
        public string? Cargo { get; set; }
        public bool EsPresidente { get; set; }
        public string Voto { get; set; } = null!;
        public string Motivacion { get; set; } = null!;
        public bool? CertificaPresupuesto { get; set; }
        public DateTime Fecha { get; set; }
    }

    // Estado de la votación visto por el usuario que consulta
    public class VotacionResponse
    {
        public int MiembrosActivos { get; set; }
        public int Quorum { get; set; }
        public int VotosEmitidos { get; set; }
        public int AFavor { get; set; }
        public int EnContra { get; set; }
        public int Abstenciones { get; set; }
        public bool PresupuestoCertificado { get; set; }
        public bool EsMiembro { get; set; }
        public bool EsPresidente { get; set; }
        public string? MiCargo { get; set; }
        public bool PuedeVotar { get; set; }
        public string? MotivoNoPuedeVotar { get; set; }
        public bool PuedeCerrar { get; set; }
        public VotoResponse? MiVoto { get; set; }
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
        public int? RequisitosPromocionId { get; set; }
        public DateTime? FechaEnvioComision { get; set; }
        public string? NumeroResolucion { get; set; }
        public string? ResueltoPor { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? Observacion { get; set; }

        // Solo en el detalle
        public DictamenPromocion? Dictamen { get; set; }
        public List<RevisionDocumentoResponse>? Revisiones { get; set; }
        public List<DocumentoSolicitudResponse>? Documentos { get; set; }
        public List<VotoResponse>? Votos { get; set; }
        public VotacionResponse? Votacion { get; set; }
    }

    public class MiembroComisionRequest
    {
        public int UsuarioId { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public bool EsPresidente { get; set; }
        public DateTime? VigenteDesde { get; set; }
    }
}
