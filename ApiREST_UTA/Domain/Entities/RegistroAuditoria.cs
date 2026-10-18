namespace ApiREST_UTA.Domain.Entities
{
    // Registro de la bitácora de auditoría (DITIC.dbo.Auditoria)
    public class RegistroAuditoria
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string? Usuario { get; set; }
        public string Accion { get; set; } = null!;
        public string Metodo { get; set; } = null!;
        public string Ruta { get; set; } = null!;
        public int Estado { get; set; }
        public string? Ip { get; set; }
        public string? Detalle { get; set; }
    }

    public class FiltroAuditoria
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? UsuarioId { get; set; }
        public string? Accion { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 50;
    }

    // Informe de funcionamiento y utilización (Reglamento DITIC art. 18 c)
    public class ReporteUso
    {
        public DateTime GeneradoEn { get; set; }
        public Dictionary<string, int> SolicitudesPorEstado { get; set; } = new();
        public double? DiasPromedioHastaResolucion { get; set; }
        public List<ConteoMensual> SolicitudesPorMes { get; set; } = new();
        public int UsuariosActivos { get; set; }
        public int UsuariosDesactivados { get; set; }
        public int IngresosExitosos30Dias { get; set; }
        public int IngresosFallidos30Dias { get; set; }
        public int OperacionesEscritura30Dias { get; set; }
        public int ErroresServidor30Dias { get; set; }
    }

    public class ConteoMensual
    {
        public string Mes { get; set; } = null!;
        public int Cantidad { get; set; }
    }
}
