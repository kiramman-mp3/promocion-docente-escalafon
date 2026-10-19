using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Application.Services.Promocion
{
    // Todo lo que el motor necesita para evaluar una promoción, ya cargado desde las bases de origen
    public class ContextoPromocion
    {
        public string Cedula { get; set; } = null!;
        public DateTime FechaCorte { get; set; } = DateTime.Today;
        public List<Rol> Roles { get; set; } = new();
        public Rol RolActual { get; set; } = null!;
        public Rol? RolObjetivo { get; set; }
        public int TiempoRolId { get; set; }
        public DateTime FechaInicioGrado { get; set; }
        public RequisitoPromocion? Requisitos { get; set; }
        public TrayectoriaDocente? Trayectoria { get; set; }
        public List<CargoAutoridad> CargosAutoridad { get; set; } = new();
        public List<Sabatico> Sabaticos { get; set; } = new();
        public List<Obra> Obras { get; set; } = new();
        public List<EvaluacionDocente> Evaluaciones { get; set; } = new();
        public List<CursoCapacitacion> Cursos { get; set; } = new();
        public List<Equivalencia> Equivalencias { get; set; } = new();
        public List<ProyectoParticipacion> Proyectos { get; set; } = new();
        public List<TrabajoTitulacion> TrabajosTitulacion { get; set; } = new();

        // Meses como autoridad dentro de la permanencia en el grado actual (periodo considerado para la promoción)
        public int MesesAutoridadEnGrado(bool soloArticulo77f) =>
            CargosAutoridad
                .Where(c => !soloArticulo77f || c.AplicaArticulo77f)
                .Sum(c => Fechas.MesesSuperpuestos(c.FechaInicio, c.FechaFin, FechaInicioGrado, FechaCorte));

        public int MesesAutoridadEnVentana(bool soloArticulo77f, DateTime desde) =>
            CargosAutoridad
                .Where(c => !soloArticulo77f || c.AplicaArticulo77f)
                .Sum(c => Fechas.MesesSuperpuestos(c.FechaInicio, c.FechaFin, desde, FechaCorte));

        public bool TuvoSabaticoAprobadoEnGrado =>
            Sabaticos.Any(s => s.InformeAprobado && Fechas.MesesSuperpuestos(s.FechaInicio, s.FechaFin, FechaInicioGrado, FechaCorte) > 0);
    }

    // Resultado de un requisito del dictamen
    public class ResultadoRequisito
    {
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Articulo { get; set; } = null!;
        public bool Cumple { get; set; }
        public bool Exento { get; set; }
        public string Obtenido { get; set; } = "";
        public string Requerido { get; set; } = "";
        public string Detalle { get; set; } = "";
    }
}
