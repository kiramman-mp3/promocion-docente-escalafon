namespace ApiREST_UTA.Domain.Entities
{
    // Solicitud de promoción. Flujo (UTA art. 47; CES arts. 75, 77 l y 99; COA arts. 217-218):
    // RevisionDocumental (Talento Humano) -> EnComision (votación) -> Aprobada | Rechazada
    // Rechazada -> Apelada -> Aprobada | Rechazada (definitiva); Aprobada -> Ejecutada (acción de personal)
    public class SolicitudPromocion
    {
        public const string RevisionDocumental = "RevisionDocumental";
        public const string EnComision = "EnComision";
        public const string Aprobada = "Aprobada";
        public const string Rechazada = "Rechazada";
        public const string Apelada = "Apelada";
        public const string Ejecutada = "Ejecutada";

        public static readonly string[] Estados = { RevisionDocumental, EnComision, Aprobada, Rechazada, Apelada, Ejecutada };
        public static readonly string[] EstadosEnTramite = { RevisionDocumental, EnComision, Apelada };

        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public int RolActualId { get; set; }
        public string RolActual { get; set; } = null!;
        public string RolObjetivo { get; set; } = null!;
        public string Estado { get; set; } = RevisionDocumental;
        public int SolicitadoPorId { get; set; }
        public string? SolicitadoPor { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int? RequisitosPromocionId { get; set; }
        public string? DictamenJson { get; set; }

        // Envío a la Comisión Académica
        public DateTime? FechaEnvioComision { get; set; }
        public int? EnviadaComisionPorId { get; set; }

        // Resolución de la Comisión (acto administrativo, CES art. 77 l)
        public string? NumeroResolucion { get; set; }
        public int? ResueltoPorId { get; set; }
        public string? ResueltoPor { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? Observacion { get; set; }
        public string? ActoHash { get; set; }

        // Ejecución: acción de personal registrada por Talento Humano
        public string? AccionPersonalNumero { get; set; }
        public DateTime? AccionPersonalFecha { get; set; }
        public int? EjecutadaPorId { get; set; }
        public DateTime? FechaEjecucion { get; set; }

        public bool EnTramite => EstadosEnTramite.Contains(Estado);

        // El docente puede anexar evidencias mientras la solicitud está en revisión documental
        public bool AdmiteEvidencias => Estado == RevisionDocumental;

        // La Comisión Académica ve la solicitud desde que Talento Humano se la envía
        public bool LlegoAComision => FechaEnvioComision.HasValue || Estado is not RevisionDocumental;
    }

    // Miembro de la Comisión Académica de promoción (UTA art. 47)
    public class MiembroComision
    {
        public const string VicerrectorAcademico = "VicerrectorAcademico";
        public const string ProfesorHCU = "ProfesorHCU";
        public const string ProfesorTerna = "ProfesorTerna";
        public const string DirectorFinanciero = "DirectorFinanciero";
        public const string DirectorTalentoHumano = "DirectorTalentoHumano";

        // Cuántos miembros de cada cargo integran la comisión
        public static readonly Dictionary<string, int> Integrantes = new()
        {
            [VicerrectorAcademico] = 1,
            [ProfesorHCU] = 1,
            [ProfesorTerna] = 2,
            [DirectorFinanciero] = 1,
            [DirectorTalentoHumano] = 1
        };

        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Cedula { get; set; }
        public string Cargo { get; set; } = null!;
        public bool EsPresidente { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime? VigenteHasta { get; set; }
    }

    public class VotoComision
    {
        public const string AFavor = "AFavor";
        public const string EnContra = "EnContra";
        public const string Abstencion = "Abstencion";
        public static readonly string[] Opciones = { AFavor, EnContra, Abstencion };

        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public int MiembroId { get; set; }
        public string? Miembro { get; set; }
        public string? Cargo { get; set; }
        public bool EsPresidente { get; set; }
        public string Voto { get; set; } = null!;
        public string Motivacion { get; set; } = null!;
        public bool? CertificaPresupuesto { get; set; }
        public DateTime Fecha { get; set; }
    }
}
