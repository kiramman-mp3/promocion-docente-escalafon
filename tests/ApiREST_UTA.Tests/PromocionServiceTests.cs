using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Domain.Interfaces.ApiREST_UTA.Domain.Interfaces;
using Moq;

namespace ApiREST_UTA.Tests
{
    public class PromocionServiceTests
    {
        // Requisitos de Agregado 2 en los datos de prueba (database/seed/02-rrhh.sql)
        private static readonly Rol Agregado2 = new()
        {
            Id = 4,
            Nombre = "Agregado 2",
            RequiereAniosEnRol = 4,
            RequiereObras = 3,
            RequiereEvaluacion = 75,
            RequiereCapacitacionHoras = 128,
            RequiereInvestigacionMeses = 6
        };

        private class Escenario
        {
            public int AniosEnRol = 6;
            public int Obras = 4;
            public decimal[] Evaluaciones = { 86.5m, 88m, 90.25m, 89.75m, 91m };
            public decimal[] PuntajeCursos = { 40, 48, 64 };
            public int[] HorasInvestigacion = { 80, 60 };
            public Rol RolObjetivo = Agregado2;

            public PromocionService Servicio()
            {
                var rol = new Mock<IRolRepository>();
                rol.Setup(r => r.ObtenerSiguienteRol(It.IsAny<int>())).ReturnsAsync(RolObjetivo);

                var rrhh = new Mock<IRRHHRepository>();
                rrhh.Setup(r => r.ObtenerTiempoRolPorCedulaAsync(It.IsAny<string>())).ReturnsAsync(new TiempoRolResponse
                {
                    Cedula = Usuarios.CedulaAna,
                    RolActual = "Agregado 1",
                    AniosEnRol = AniosEnRol,
                    FechaInicio = new DateTime(2020, 3, 1),
                    PdfBase64 = ""
                });

                var obras = new Mock<IObraRepository>();
                obras.Setup(r => r.GetObrasPorCedulaAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(Enumerable.Range(1, Obras).Select(i => new ObraResponse { Id = i }).ToList());

                var evaluaciones = new Mock<IEvaluacionDocenteRepository>();
                evaluaciones.Setup(r => r.ObtenerEvaluacionesPorCedula(It.IsAny<string>()))
                    .ReturnsAsync(Evaluaciones.Select(p => new EvaluacionDocente { Puntaje = p }).ToList());

                var cursos = new Mock<ICursoRepository>();
                cursos.Setup(r => r.ObtenerCursosPorCedula(It.IsAny<string>()))
                    .ReturnsAsync(PuntajeCursos.Select(p => new CursoCapacitacion { Puntaje = p }).ToList());

                var investigaciones = new Mock<IInvestigacionRepository>();
                investigaciones.Setup(r => r.ObtenerInvestigacionesPorCedula(It.IsAny<string>()))
                    .ReturnsAsync(HorasInvestigacion.Select(h => new Investigacion { Horas = h }).ToList());

                return new PromocionService(rol.Object, rrhh.Object, obras.Object, evaluaciones.Object, cursos.Object, investigaciones.Object);
            }
        }

        private static Task<RequisitoResponse> Validar(Escenario e) =>
            e.Servicio().ValidarPromocionCompleta(new RequisitoRequest { Cedula = Usuarios.CedulaAna, RolActualId = 3 });

        [Fact]
        public async Task DocenteQueCumpleTodo_NoTieneObservaciones()
        {
            var r = await Validar(new Escenario());

            Assert.Equal("Agregado 2", r.RolObjetivo);
            Assert.True(r.CumpleExperiencia);
            Assert.True(r.CumpleObras);
            Assert.True(r.CumpleEvaluacion);
            Assert.True(r.CumpleCapacitacion);
            Assert.True(r.CumpleInvestigacion);
            Assert.True(r.CumpleTesisDoctorado);
            Assert.Equal("", r.Observaciones);
        }

        [Fact]
        public async Task PocosAniosYPocasObras_NoCumpleYLoExplica()
        {
            var r = await Validar(new Escenario { AniosEnRol = 2, Obras = 1 });

            Assert.False(r.CumpleExperiencia);
            Assert.False(r.CumpleObras);
            Assert.Contains("tiempo mínimo", r.Observaciones);
            Assert.Contains("obras", r.Observaciones);
        }

        [Fact]
        public async Task MenosDeCuatroEvaluaciones_NoCumpleAunqueElPromedioSeaAlto()
        {
            var r = await Validar(new Escenario { Evaluaciones = new[] { 99m, 98m, 97m } });

            Assert.False(r.CumpleEvaluacion);
        }

        [Fact]
        public async Task PromedioDeEvaluacionBajo_NoCumple()
        {
            var r = await Validar(new Escenario { Evaluaciones = new[] { 70m, 72m, 74m, 71m } });

            Assert.False(r.CumpleEvaluacion);
        }

        [Fact]
        public async Task Investigacion_CuentaVeinteHorasPorMes()
        {
            // Agregado 2 pide 6 meses = 120 horas
            Assert.True((await Validar(new Escenario { HorasInvestigacion = new[] { 120 } })).CumpleInvestigacion);
            Assert.False((await Validar(new Escenario { HorasInvestigacion = new[] { 119 } })).CumpleInvestigacion);
        }

        [Fact]
        public async Task Capacitacion_SumaElPuntajeDeLosCursos()
        {
            Assert.True((await Validar(new Escenario { PuntajeCursos = new[] { 128m } })).CumpleCapacitacion);
            Assert.False((await Validar(new Escenario { PuntajeCursos = new[] { 100m } })).CumpleCapacitacion);
        }
    }
}
