namespace ApiREST_UTA.Domain.Entities
{
    public class SolicitudPromocion
    {
        public const string Pendiente = "Pendiente";
        public const string Aprobada = "Aprobada";
        public const string Rechazada = "Rechazada";

        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public int RolActualId { get; set; }
        public string RolActual { get; set; } = null!;
        public string RolObjetivo { get; set; } = null!;
        public string Estado { get; set; } = Pendiente;
        public int SolicitadoPorId { get; set; }
        public string? SolicitadoPor { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int? ResueltoPorId { get; set; }
        public string? ResueltoPor { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? Observacion { get; set; }
    }
}
