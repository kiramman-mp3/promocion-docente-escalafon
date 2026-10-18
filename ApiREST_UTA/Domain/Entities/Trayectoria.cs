namespace ApiREST_UTA.Domain.Entities
{
    // Trayectoria del docente en RRHH (RRHH.dbo.TrayectoriaDocente)
    public class TrayectoriaDocente
    {
        // UTA 0153-HCU-2026, transitoria quinta; CES, transitoria décima primera
        public static readonly DateTime CorteRegimenAnterior = new(2012, 11, 7);

        public string Cedula { get; set; } = null!;
        public DateTime FechaIngresoInstitucion { get; set; }
        public DateTime FechaIngresoTitular { get; set; }
        public int MesesNoTitular { get; set; }
        public int MesesTecnicoDocente { get; set; }
        public bool IngresoConPhD { get; set; }

        public bool IngresoAntesDe2012 => FechaIngresoInstitucion < CorteRegimenAnterior;
    }

    // Cargo de autoridad (RRHH.dbo.CargosAutoridad)
    public class CargoAutoridad
    {
        public const string Rector = "Rector";
        public const string Vicerrector = "Vicerrector";
        public const string AutoridadOrganismoSES = "AutoridadOrganismoSES";
        public const string AutoridadAcademica = "AutoridadAcademica";

        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public string Cargo { get; set; } = null!;
        public string TipoCargo { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // CES art. 77 f: rector, vicerrector o autoridad de un organismo público del Sistema de Educación Superior
        public bool AplicaArticulo77f => TipoCargo is Rector or Vicerrector or AutoridadOrganismoSES;
    }

    // Periodo sabático (RRHH.dbo.Sabaticos)
    public class Sabatico
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool InformeAprobado { get; set; }
    }

    // Trabajo de titulación dirigido o codirigido (DAC.dbo.TrabajosTitulacionDirigidos)
    public class TrabajoTitulacion
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string Nivel { get; set; } = RequisitoPromocion.NivelGrado;
        public string Rol { get; set; } = "Director";
        public DateTime FechaAprobacion { get; set; }
        public string Universidad { get; set; } = null!;

        public static int Rango(string nivel) => nivel switch
        {
            RequisitoPromocion.NivelDoctorado => 3,
            RequisitoPromocion.NivelMaestria => 2,
            _ => 1
        };
    }

    // Equivalencias del CES art. 77 g-j (DEaDV.dbo.Equivalencias)
    public class Equivalencia
    {
        public const string EvaluadorExternoConcurso = "EvaluadorExternoConcurso";
        public const string ParEvaluadorProyectos = "ParEvaluadorProyectos";
        public const string FacilitadorCESGradoMaestria = "FacilitadorCESGradoMaestria";
        public const string FacilitadorCESDoctorado = "FacilitadorCESDoctorado";
        public const string FacilitadorCACES = "FacilitadorCACES";
        public const string EvaluacionAcademicaCES = "EvaluacionAcademicaCES";

        public int Id { get; set; }
        public string Cedula { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public int? Meses { get; set; }
        public string Descripcion { get; set; } = null!;

        public bool EsFacilitacionCES => Tipo is FacilitadorCESGradoMaestria or FacilitadorCESDoctorado;

        // Horas de capacitación que reconoce (art. 77 g, h, i). La regla R5 aplica los topes por promoción.
        public int HorasCapacitacion => Tipo switch
        {
            EvaluadorExternoConcurso or ParEvaluadorProyectos => 16,
            FacilitadorCESGradoMaestria => 24,
            FacilitadorCESDoctorado => 32,
            FacilitadorCACES => Math.Min(Meses ?? 0, 2) * 16,
            _ => 0
        };
    }
}
