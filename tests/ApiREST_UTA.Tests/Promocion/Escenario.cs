using ApiREST_UTA.Application.Services.Promocion;
using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Tests.Promocion
{
    // Contexto base: docente Agregado 1 (grado 3) desde hace 5 años que cumple todo para Agregado 2.
    // Cada prueba cambia solo lo que necesita.
    public static class Escenario
    {
        public static readonly DateTime Hoy = new(2026, 10, 17);

        public static List<Rol> Roles() => new()
        {
            new() { Id = 1, Nombre = "Auxiliar 1", PermanenciaAnios = 4, Promocionable = true },
            new() { Id = 2, Nombre = "Auxiliar 2", PermanenciaAnios = 4, Promocionable = true },
            new() { Id = 3, Nombre = "Agregado 1", PermanenciaAnios = 4, Promocionable = true },
            new() { Id = 4, Nombre = "Agregado 2", PermanenciaAnios = 4, Promocionable = true },
            new() { Id = 5, Nombre = "Agregado 3", PermanenciaAnios = 4, Promocionable = false },
            new() { Id = 6, Nombre = "Principal 1", PermanenciaAnios = 3, Promocionable = true },
            new() { Id = 7, Nombre = "Principal 2", PermanenciaAnios = 3, Promocionable = true },
            new() { Id = 8, Nombre = "Principal 3", PermanenciaAnios = null, Promocionable = false }
        };

        public static RequisitoPromocion Requisitos(int gradoOrigen = 3) => new()
        {
            Id = 10 + gradoOrigen,
            GradoOrigen = gradoOrigen,
            GradoDestino = gradoOrigen + 1,
            Normativa = "CES art. 76; UTA art. 48",
            Resolucion = "Pruebas",
            VigenteDesde = new DateTime(2026, 3, 26),
            ObrasTotales = 3,
            ObrasEnGrado = 1,
            EvaluacionMinima = 75,
            EvaluacionPeriodos = 4,
            CapacitacionHoras = 96,
            CapacitacionAnios = 3,
            CapacitacionPctPedagogica = 25,
            ProyectosMeses = 12,
            RequiereDireccionProyecto = false,
            TrabajosTitulacion = 1,
            TitulacionNivelMinimo = RequisitoPromocion.NivelGrado
        };

        public static Obra ObraRelevante(DateTime fecha, string titulo = "Artículo") => new()
        {
            Cedula = "1801234567",
            Titulo = titulo,
            FechaPublicacion = fecha,
            NombreRevista = "Revista indexada",
            TipoObra = Obra.ArticuloArbitrado,
            ISSN = "1390-0001",
            BaseIndexacion = "Scopus",
            RevisionPares = true
        };

        public static EvaluacionDocente Evaluacion(DateTime periodo, decimal puntaje, bool docencia = true) => new()
        {
            Cedula = "1801234567",
            Periodo = periodo,
            PeriodoAcademico = $"PAO {periodo:yyyy-MM}",
            Universidad = "UTA",
            Carrera = "Software",
            Puntaje = puntaje,
            EjercioDocencia = docencia
        };

        public static CursoCapacitacion Curso(int mesesAtras, int horas, string tipo = CursoCapacitacion.Cientifica) => new()
        {
            Cedula = "1801234567",
            Universidad = "UTA",
            Facultad = "FISEI",
            Horas = horas,
            TipoCapacitacion = tipo,
            FechaEvaluacion = Hoy.AddMonths(-mesesAtras),
            FechaFin = Hoy.AddMonths(-mesesAtras)
        };

        public static ContextoPromocion Base(int gradoActual = 3)
        {
            var roles = Roles();
            var rol = roles.First(r => r.Id == gradoActual);
            return new ContextoPromocion
            {
                Cedula = "1801234567",
                FechaCorte = Hoy,
                Roles = roles,
                RolActual = rol,
                RolObjetivo = rol.Promocionable ? roles.First(r => r.Id == gradoActual + 1) : null,
                FechaInicioGrado = Hoy.AddYears(-5),
                Requisitos = rol.Promocionable ? Requisitos(gradoActual) : null,
                Trayectoria = new TrayectoriaDocente
                {
                    Cedula = "1801234567",
                    FechaIngresoInstitucion = new DateTime(2015, 4, 1),
                    FechaIngresoTitular = new DateTime(2016, 3, 1)
                },
                Obras = new()
                {
                    ObraRelevante(Hoy.AddYears(-7)),
                    ObraRelevante(Hoy.AddYears(-4)),
                    ObraRelevante(Hoy.AddYears(-2))
                },
                Evaluaciones = new()
                {
                    Evaluacion(Hoy.AddMonths(-30), 86),
                    Evaluacion(Hoy.AddMonths(-24), 88),
                    Evaluacion(Hoy.AddMonths(-18), 90),
                    Evaluacion(Hoy.AddMonths(-12), 92)
                },
                Cursos = new()
                {
                    Curso(6, 40, CursoCapacitacion.Pedagogica),
                    Curso(14, 60)
                },
                Proyectos = new()
                {
                    new ProyectoParticipacion { Cedula = "1801234567", NombreProyecto = "Proyecto A", Rol = ProyectoParticipacion.Participante, Meses = 12 }
                },
                TrabajosTitulacion = new()
                {
                    new TrabajoTitulacion { Cedula = "1801234567", Titulo = "Tesis", Nivel = RequisitoPromocion.NivelGrado, Universidad = "UTA", FechaAprobacion = Hoy.AddYears(-1) }
                }
            };
        }

        public static ResultadoRequisito Resultado(ContextoPromocion c, string codigo) =>
            new MotorPromocion().Evaluar(c).Requisito(codigo)!;
    }
}
