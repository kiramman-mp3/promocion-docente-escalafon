using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Tests.Promocion;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ApiREST_UTA.Tests
{
    // Flujo de la solicitud: presentación (CES art. 99), revisión documental, Comisión Académica (UTA art. 47)
    public class SolicitudServiceTests
    {
        private const string CedulaJorge = "1805555555";

        private readonly Mock<ISolicitudRepository> _solicitudes = new();
        private readonly Mock<IComisionRepository> _comision = new();
        private readonly Mock<IRRHHRepository> _rrhh = new();
        private readonly Mock<IRequisitoRepository> _requisitos = new();
        private readonly Mock<IGeneradorActo> _actos = new();
        private readonly Mock<IUsuarioRepository> _usuarios = new();

        private static readonly MiembroComision Presidente = new() { Id = 1, UsuarioId = 10, Cargo = MiembroComision.VicerrectorAcademico, EsPresidente = true };
        private static readonly MiembroComision Financiero = new() { Id = 5, UsuarioId = 11, Cargo = MiembroComision.DirectorFinanciero };
        private static readonly MiembroComision ProfesoraLucia = new() { Id = 3, UsuarioId = 5, Cargo = MiembroComision.ProfesorTerna, Cedula = "1712345678" };

        public SolicitudServiceTests()
        {
            _rrhh.Setup(r => r.ObtenerTiempoRolPorCedulaAsync(Usuarios.CedulaAna)).ReturnsAsync(new TiempoRolResponse
            { Id = 1, Cedula = Usuarios.CedulaAna, RolActual = "Agregado 1", FechaInicio = DateTime.Today.AddYears(-6), PdfBase64 = "" });
            _rrhh.Setup(r => r.ObtenerTiempoRolPorCedulaAsync(Usuarios.CedulaCarlos)).ReturnsAsync(new TiempoRolResponse
            { Id = 2, Cedula = Usuarios.CedulaCarlos, RolActual = "Auxiliar 2", FechaInicio = DateTime.Today.AddMonths(-30), PdfBase64 = "" });
            _rrhh.Setup(r => r.ObtenerTiempoRolPorCedulaAsync(CedulaJorge)).ReturnsAsync(new TiempoRolResponse
            { Id = 3, Cedula = CedulaJorge, RolActual = "Agregado 3", FechaInicio = DateTime.Today.AddYears(-7), PdfBase64 = "" });
            _rrhh.Setup(r => r.ObtenerCargosAutoridadAsync(It.IsAny<string>())).ReturnsAsync(new List<CargoAutoridad>());
            _rrhh.Setup(r => r.ObtenerSabaticosAsync(It.IsAny<string>())).ReturnsAsync(new List<Sabatico>());

            _requisitos.Setup(r => r.ObtenerVigenteAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync((int grado, DateTime _) => Escenario.Requisitos(grado));
            _requisitos.Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => Escenario.Requisitos(id - 10));

            _solicitudes.Setup(r => r.ListarAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>())).ReturnsAsync(new List<SolicitudPromocion>());
            _solicitudes.Setup(r => r.ObtenerRevisionesAsync(It.IsAny<int>())).ReturnsAsync(new List<RevisionDocumento>());
            _comision.Setup(r => r.ObtenerVotosAsync(It.IsAny<int>())).ReturnsAsync(new List<VotoComision>());
            _comision.Setup(r => r.ListarMiembrosAsync(It.IsAny<bool>())).ReturnsAsync(new List<MiembroComision> { Presidente, Financiero, ProfesoraLucia });
        }

        private SolicitudService Servicio()
        {
            var roles = new Mock<IRolRepository>();
            roles.Setup(r => r.ListarAsync()).ReturnsAsync(Escenario.Roles());

            Mock<T> Vacio<T>() where T : class => new();
            var obras = Vacio<IObraRepository>();
            obras.Setup(r => r.ObtenerObrasAsync(It.IsAny<string>())).ReturnsAsync(new List<Obra>());
            var evaluaciones = Vacio<IEvaluacionDocenteRepository>();
            evaluaciones.Setup(r => r.ObtenerEvaluacionesPorCedula(It.IsAny<string>())).ReturnsAsync(new List<EvaluacionDocente>());
            var cursos = Vacio<ICursoRepository>();
            cursos.Setup(r => r.ObtenerCursosPorCedula(It.IsAny<string>())).ReturnsAsync(new List<CursoCapacitacion>());
            var equivalencias = Vacio<IEquivalenciaRepository>();
            equivalencias.Setup(r => r.ObtenerEquivalenciasAsync(It.IsAny<string>())).ReturnsAsync(new List<Equivalencia>());
            var proyectos = Vacio<IProyectoPonderadoRepository>();
            proyectos.Setup(r => r.ObtenerProyectosAsync(It.IsAny<string>())).ReturnsAsync(new List<ProyectoParticipacion>());
            var tesis = Vacio<ITesisRepository>();
            tesis.Setup(r => r.ObtenerTrabajosTitulacionAsync(It.IsAny<string>())).ReturnsAsync(new List<TrabajoTitulacion>());
            var investigaciones = Vacio<IInvestigacionRepository>();
            investigaciones.Setup(r => r.ObtenerInvestigacionesPorCedula(It.IsAny<string>())).ReturnsAsync(new List<Investigacion>());
            var evidencias = Vacio<IEvidenciaRepository>();
            evidencias.Setup(r => r.ListarAsync(It.IsAny<int>())).ReturnsAsync(new List<EvidenciaSolicitud>());

            var promocion = new PromocionService(roles.Object, _rrhh.Object, obras.Object, evaluaciones.Object, cursos.Object,
                equivalencias.Object, proyectos.Object, tesis.Object, _requisitos.Object);
            _actos.Setup(a => a.Generar(It.IsAny<DatosActo>())).Returns(System.Text.Encoding.ASCII.GetBytes("%PDF-1.4 acto"));
            return new SolicitudService(_solicitudes.Object, promocion, _comision.Object, investigaciones.Object, evidencias.Object,
                _actos.Object, _usuarios.Object);
        }

        private void ConSolicitud(int id, string estado, string cedula = Usuarios.CedulaAna) =>
            _solicitudes.Setup(r => r.ObtenerPorIdAsync(id)).ReturnsAsync(new SolicitudPromocion
            {
                Id = id,
                Cedula = cedula,
                RolActualId = 3,
                RolActual = "Agregado 1",
                RolObjetivo = "Agregado 2",
                Estado = estado,
                RequisitosPromocionId = 13,
                FechaEnvioComision = estado == SolicitudPromocion.RevisionDocumental ? null : DateTime.UtcNow
            });

        private void ComoMiembro(MiembroComision miembro) =>
            _comision.Setup(r => r.ObtenerMiembroActivoPorUsuarioAsync(miembro.UsuarioId)).ReturnsAsync(miembro);

        private static async Task<int> CodigoDeError(Func<Task> accion) =>
            (await Assert.ThrowsAsync<ReglaNegocioException>(accion)).StatusCode;

        private static VotoComision Voto(MiembroComision m, string voto, bool? certifica = null) =>
            new() { MiembroId = m.Id, Cargo = m.Cargo, EsPresidente = m.EsPresidente, Voto = voto, Motivacion = "x", CertificaPresupuesto = certifica };

        // ---------- Presentación (CES art. 99) ----------

        [Fact]
        public async Task Art99_Crear_GuardaFotoDeRequisitosYDictamen()
        {
            _solicitudes.Setup(r => r.CrearAsync(It.IsAny<SolicitudPromocion>())).ReturnsAsync(10);
            ConSolicitud(10, SolicitudPromocion.RevisionDocumental);

            var creada = await Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente(id: 3));

            Assert.Equal(10, creada.Id);
            _solicitudes.Verify(r => r.CrearAsync(It.Is<SolicitudPromocion>(s =>
                s.Cedula == Usuarios.CedulaAna && s.RolObjetivo == "Agregado 2" && s.SolicitadoPorId == 3 &&
                s.RequisitosPromocionId == 13 && s.DictamenJson != null)));
        }

        [Fact]
        public async Task Art76_Agregado3_NoPuedeSolicitarPromocion()
        {
            var error = await Assert.ThrowsAsync<ReglaNegocioException>(() => Servicio().CrearAsync(CedulaJorge, Usuarios.Rrhh()));

            Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
            Assert.Contains("concurso", error.Message);
        }

        [Fact]
        public async Task Art99_SinPermanenciaMinima_NoPuedeSolicitar()
        {
            var error = await Assert.ThrowsAsync<ReglaNegocioException>(() => Servicio().CrearAsync(Usuarios.CedulaCarlos, Usuarios.Docente(cedula: Usuarios.CedulaCarlos)));

            Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
            Assert.Contains("permanencia", error.Message);
        }

        [Fact]
        public async Task Crear_ConSolicitudEnTramite_Da409()
        {
            _solicitudes.Setup(r => r.ExisteEnTramiteAsync(Usuarios.CedulaAna)).ReturnsAsync(true);

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente())));
        }

        [Fact]
        public async Task Crear_PromocionYaAprobada_Da409()
        {
            _solicitudes.Setup(r => r.ListarAsync(null, Usuarios.CedulaAna, false)).ReturnsAsync(new List<SolicitudPromocion>
            {
                new() { Cedula = Usuarios.CedulaAna, RolActual = "Agregado 1", RolObjetivo = "Agregado 2", Estado = SolicitudPromocion.Aprobada }
            });

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente())));
            _solicitudes.Verify(r => r.CrearAsync(It.IsAny<SolicitudPromocion>()), Times.Never);
        }

        // ---------- Revisión documental ----------

        [Fact]
        public async Task RevisarDocumento_QueNoEsDelExpediente_Da400()
        {
            ConSolicitud(1, SolicitudPromocion.RevisionDocumental);
            var request = new RevisarDocumentoRequest { TipoDocumento = "Obra", DocumentoId = 999, Estado = "Aprobado" };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().RevisarDocumentoAsync(1, request, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task RevisarDocumento_RechazoSinMotivo_Da400()
        {
            ConSolicitud(1, SolicitudPromocion.RevisionDocumental);
            var request = new RevisarDocumentoRequest { TipoDocumento = "TiempoRol", DocumentoId = 1, Estado = "Rechazado" };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().RevisarDocumentoAsync(1, request, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task RevisarDocumento_DelExpediente_SeGuarda()
        {
            ConSolicitud(1, SolicitudPromocion.RevisionDocumental);
            var request = new RevisarDocumentoRequest { TipoDocumento = "TiempoRol", DocumentoId = 1, Estado = "Aprobado" };

            await Servicio().RevisarDocumentoAsync(1, request, Usuarios.Rrhh(id: 2));

            _solicitudes.Verify(r => r.GuardarRevisionAsync(It.Is<RevisionDocumento>(rv =>
                rv.SolicitudId == 1 && rv.TipoDocumento == "TiempoRol" && rv.DocumentoId == 1 && rv.RevisadoPorId == 2)));
        }

        [Fact]
        public async Task EnviarAComision_ConDocumentosSinRevisar_Da409()
        {
            ConSolicitud(1, SolicitudPromocion.RevisionDocumental);

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().EnviarAComisionAsync(1, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task EnviarAComision_ConTodoRevisado_GuardaElDictamenDefinitivo()
        {
            ConSolicitud(1, SolicitudPromocion.RevisionDocumental);
            _solicitudes.Setup(r => r.ObtenerRevisionesAsync(1)).ReturnsAsync(new List<RevisionDocumento>
            {
                new() { SolicitudId = 1, TipoDocumento = "TiempoRol", DocumentoId = 1, Estado = RevisionDocumento.Aprobado }
            });
            _solicitudes.Setup(r => r.EnviarAComisionAsync(1, 2, It.IsAny<string>())).ReturnsAsync(true);

            await Servicio().EnviarAComisionAsync(1, Usuarios.Rrhh(id: 2));

            _solicitudes.Verify(r => r.EnviarAComisionAsync(1, 2, It.Is<string>(json => json.Contains("Agregado 2"))));
        }

        // ---------- Comisión Académica (UTA art. 47) ----------

        [Fact]
        public async Task Art47_QuienNoIntegraLaComision_NoVota()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision);
            var voto = new VotoRequest { Voto = VotoComision.AFavor, Motivacion = "Cumple" };

            Assert.Equal(StatusCodes.Status403Forbidden, await CodigoDeError(() => Servicio().VotarAsync(1, voto, Usuarios.Rrhh(id: 99))));
        }

        [Fact]
        public async Task Art47_UnMiembroNoVotaSuPropiaSolicitud()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision, cedula: "1712345678");
            ComoMiembro(ProfesoraLucia);
            var voto = new VotoRequest { Voto = VotoComision.AFavor, Motivacion = "Cumple" };

            Assert.Equal(StatusCodes.Status403Forbidden, await CodigoDeError(() =>
                Servicio().VotarAsync(1, voto, Usuarios.Docente(id: ProfesoraLucia.UsuarioId, cedula: "1712345678"))));
        }

        [Fact]
        public async Task Art47_SoloElDirectorFinancieroCertificaPresupuesto()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision);
            ComoMiembro(Presidente);
            var voto = new VotoRequest { Voto = VotoComision.AFavor, Motivacion = "Cumple", CertificaPresupuesto = true };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().VotarAsync(1, voto, Usuarios.Administrador(id: Presidente.UsuarioId))));
        }

        [Fact]
        public async Task Art47_VotoSinMotivacion_Da400()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision);
            ComoMiembro(Financiero);
            var voto = new VotoRequest { Voto = VotoComision.AFavor, Motivacion = " " };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().VotarAsync(1, voto, Usuarios.Rrhh(id: Financiero.UsuarioId))));
        }

        [Fact]
        public async Task Art47_SoloLaPresidenciaCierraLaVotacion()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision);
            ComoMiembro(Financiero);

            Assert.Equal(StatusCodes.Status403Forbidden, await CodigoDeError(() =>
                Servicio().CerrarVotacionAsync(1, new CerrarVotacionRequest { Motivacion = "x" }, Usuarios.Rrhh(id: Financiero.UsuarioId))));
        }

        [Fact]
        public async Task COA100_CerrarSinMotivacion_Da400()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision);
            ComoMiembro(Presidente);

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() =>
                Servicio().CerrarVotacionAsync(1, new CerrarVotacionRequest { Motivacion = "" }, Usuarios.Administrador(id: Presidente.UsuarioId))));
        }

        [Fact]
        public void Art47_SinQuorum_NoSeResuelve()
        {
            var votos = new List<VotoComision> { Voto(Presidente, VotoComision.AFavor), Voto(Financiero, VotoComision.AFavor, true) };

            Assert.Equal(StatusCodes.Status409Conflict, Assert.Throws<ReglaNegocioException>(() => SolicitudService.ResultadoVotacion(votos)).StatusCode);
        }

        [Fact]
        public void Art47_MayoriaAFavorConPresupuesto_Aprueba()
        {
            var votos = new List<VotoComision>
            {
                Voto(Presidente, VotoComision.AFavor), Voto(Financiero, VotoComision.AFavor, true),
                Voto(new MiembroComision { Id = 7 }, VotoComision.AFavor), Voto(new MiembroComision { Id = 8 }, VotoComision.EnContra)
            };

            Assert.Equal(SolicitudPromocion.Aprobada, SolicitudService.ResultadoVotacion(votos).Estado);
        }

        [Fact]
        public void Art99_SinCertificacionPresupuestaria_NoSeAprueba()
        {
            var votos = new List<VotoComision>
            {
                Voto(Presidente, VotoComision.AFavor), Voto(Financiero, VotoComision.AFavor, false),
                Voto(new MiembroComision { Id = 7 }, VotoComision.AFavor), Voto(new MiembroComision { Id = 8 }, VotoComision.AFavor)
            };

            Assert.Equal(StatusCodes.Status409Conflict, Assert.Throws<ReglaNegocioException>(() => SolicitudService.ResultadoVotacion(votos)).StatusCode);
        }

        [Theory]
        [InlineData(VotoComision.AFavor, SolicitudPromocion.Aprobada)]
        [InlineData(VotoComision.EnContra, SolicitudPromocion.Rechazada)]
        [InlineData(VotoComision.Abstencion, SolicitudPromocion.Rechazada)]
        public void Art47_EmpateLoDecideElVotoDirimenteDeLaPresidencia(string votoPresidente, string esperado)
        {
            var votos = new List<VotoComision>
            {
                Voto(Presidente, votoPresidente),
                Voto(Financiero, VotoComision.AFavor, true),
                Voto(new MiembroComision { Id = 7 }, VotoComision.AFavor),
                Voto(new MiembroComision { Id = 8 }, VotoComision.EnContra),
                Voto(new MiembroComision { Id = 9 }, VotoComision.EnContra)
            };
            if (votoPresidente == VotoComision.AFavor)
                votos.Add(Voto(new MiembroComision { Id = 6 }, VotoComision.EnContra));

            Assert.Equal(esperado, SolicitudService.ResultadoVotacion(votos).Estado);
        }

        [Fact]
        public void Art47_MayoriaEnContra_Rechaza()
        {
            var votos = new List<VotoComision>
            {
                Voto(Presidente, VotoComision.EnContra), Voto(Financiero, VotoComision.EnContra, false),
                Voto(new MiembroComision { Id = 7 }, VotoComision.EnContra), Voto(new MiembroComision { Id = 8 }, VotoComision.AFavor)
            };

            Assert.Equal(SolicitudPromocion.Rechazada, SolicitudService.ResultadoVotacion(votos).Estado);
        }

        // ---------- Acto administrativo y acción de personal (CES art. 77 l) ----------

        [Fact]
        public async Task Art77l_AlCerrarLaVotacionSeEmiteElActoConSuHuella()
        {
            ConSolicitud(1, SolicitudPromocion.EnComision);
            ComoMiembro(Presidente);
            _comision.Setup(r => r.ObtenerVotosAsync(1)).ReturnsAsync(new List<VotoComision>
            {
                Voto(Presidente, VotoComision.AFavor), Voto(Financiero, VotoComision.AFavor, true),
                Voto(new MiembroComision { Id = 7 }, VotoComision.AFavor), Voto(new MiembroComision { Id = 8 }, VotoComision.AFavor)
            });
            _solicitudes.Setup(r => r.ResolverAsync(1, SolicitudPromocion.EnComision, SolicitudPromocion.Aprobada, It.IsAny<string>(), It.IsAny<string?>(), Presidente.UsuarioId))
                .ReturnsAsync(true);

            await Servicio().CerrarVotacionAsync(1, new CerrarVotacionRequest { Motivacion = "Cumple los requisitos" }, Usuarios.Administrador(id: Presidente.UsuarioId));

            _actos.Verify(a => a.Generar(It.Is<DatosActo>(d => d.Solicitud.Id == 1 && d.Votos.Count == 4)));
            _solicitudes.Verify(r => r.GuardarActoAsync(1, It.IsAny<byte[]>(), It.Is<string>(h => h.Length == 64)));
        }

        [Fact]
        public async Task Art77l_AccionDePersonal_SoloEnSolicitudesAprobadas()
        {
            ConSolicitud(1, SolicitudPromocion.Rechazada);
            var request = new AccionPersonalRequest { Numero = "AP-2026-0100", Fecha = DateTime.Today };

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().RegistrarAccionPersonalAsync(1, request, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task Art77l_AccionDePersonalConFechaFutura_Da400()
        {
            ConSolicitud(1, SolicitudPromocion.Aprobada);
            var request = new AccionPersonalRequest { Numero = "AP-2026-0100", Fecha = DateTime.Today.AddDays(3) };

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().RegistrarAccionPersonalAsync(1, request, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task Art77l_AccionDePersonal_EjecutaLaPromocion()
        {
            ConSolicitud(1, SolicitudPromocion.Aprobada);
            _solicitudes.Setup(r => r.RegistrarAccionPersonalAsync(1, "AP-2026-0100", DateTime.Today, 2)).ReturnsAsync(true);

            await Servicio().RegistrarAccionPersonalAsync(1, new AccionPersonalRequest { Numero = " AP-2026-0100 ", Fecha = DateTime.Today }, Usuarios.Rrhh(id: 2));

            _solicitudes.Verify(r => r.RegistrarAccionPersonalAsync(1, "AP-2026-0100", DateTime.Today, 2));
        }

        // ---------- Visibilidad ----------

        [Fact]
        public async Task Obtener_SolicitudDeOtroDocente_Da403()
        {
            ConSolicitud(5, SolicitudPromocion.RevisionDocumental, cedula: Usuarios.CedulaCarlos);

            Assert.Equal(StatusCodes.Status403Forbidden, await CodigoDeError(() => Servicio().ObtenerAsync(5, Usuarios.Docente(cedula: Usuarios.CedulaAna))));
        }

        [Fact]
        public async Task Listar_DocenteSoloVeSusSolicitudes_RevisorVeTodas()
        {
            await Servicio().ListarAsync(null, Usuarios.Docente(cedula: Usuarios.CedulaAna));
            await Servicio().ListarAsync("EnComision", Usuarios.Rrhh());

            _solicitudes.Verify(r => r.ListarAsync(null, Usuarios.CedulaAna, false));
            _solicitudes.Verify(r => r.ListarAsync("EnComision", null, false));
        }
    }
}
