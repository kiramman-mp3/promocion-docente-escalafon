using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ApiREST_UTA.Tests
{
    public class SolicitudServiceTests
    {
        private readonly Mock<ISolicitudRepository> _solicitudes = new();
        private readonly Mock<IRRHHRepository> _rrhh = new();
        private readonly Mock<IRolRepository> _roles = new();

        private static readonly Rol Agregado1 = new() { Id = 3, Nombre = "Agregado 1" };
        private static readonly Rol Agregado2 = new() { Id = 4, Nombre = "Agregado 2" };

        public SolicitudServiceTests()
        {
            _rrhh.Setup(r => r.ObtenerTiempoRolPorCedulaAsync(Usuarios.CedulaAna))
                .ReturnsAsync(new TiempoRolResponse { Cedula = Usuarios.CedulaAna, RolActual = "Agregado 1" });
            _roles.Setup(r => r.ObtenerPorNombre("Agregado 1")).ReturnsAsync(Agregado1);
            _roles.Setup(r => r.BuscarSiguienteRol(3)).ReturnsAsync(Agregado2);
            _solicitudes.Setup(r => r.ListarAsync(It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<SolicitudPromocion>());
            _solicitudes.Setup(r => r.ObtenerRevisionesAsync(It.IsAny<int>())).ReturnsAsync(new List<RevisionDocumento>());
        }

        private SolicitudService Servicio() => new(_solicitudes.Object, _rrhh.Object, _roles.Object);

        private void ConSolicitud(int id, string estado, string cedula = Usuarios.CedulaAna) =>
            _solicitudes.Setup(r => r.ObtenerPorIdAsync(id)).ReturnsAsync(new SolicitudPromocion
            {
                Id = id,
                Cedula = cedula,
                RolActual = "Agregado 1",
                RolObjetivo = "Agregado 2",
                Estado = estado
            });

        private static async Task<int> CodigoDeError(Func<Task> accion) =>
            (await Assert.ThrowsAsync<ReglaNegocioException>(accion)).StatusCode;

        // ---------- Crear ----------

        [Fact]
        public async Task Crear_GuardaRolActualYObjetivoConElSolicitante()
        {
            _solicitudes.Setup(r => r.CrearAsync(It.IsAny<SolicitudPromocion>())).ReturnsAsync(10);
            ConSolicitud(10, SolicitudPromocion.Pendiente);

            var creada = await Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente(id: 3));

            Assert.Equal(10, creada.Id);
            _solicitudes.Verify(r => r.CrearAsync(It.Is<SolicitudPromocion>(s =>
                s.Cedula == Usuarios.CedulaAna &&
                s.RolActualId == 3 &&
                s.RolObjetivo == "Agregado 2" &&
                s.SolicitadoPorId == 3)));
        }

        [Fact]
        public async Task Crear_CedulaQueNoEstaEnRrhh_Da404()
        {
            Assert.Equal(StatusCodes.Status404NotFound,
                await CodigoDeError(() => Servicio().CrearAsync("0000000000", Usuarios.Rrhh())));
        }

        [Fact]
        public async Task Crear_EnElUltimoRol_Da400()
        {
            _roles.Setup(r => r.BuscarSiguienteRol(3)).ReturnsAsync((Rol?)null);

            Assert.Equal(StatusCodes.Status400BadRequest,
                await CodigoDeError(() => Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente())));
        }

        [Fact]
        public async Task Crear_ConSolicitudPendiente_Da409()
        {
            _solicitudes.Setup(r => r.ExistePendienteAsync(Usuarios.CedulaAna)).ReturnsAsync(true);

            Assert.Equal(StatusCodes.Status409Conflict,
                await CodigoDeError(() => Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente())));
        }

        [Fact]
        public async Task Crear_PromocionYaAprobada_Da409()
        {
            _solicitudes.Setup(r => r.ListarAsync(SolicitudPromocion.Aprobada, Usuarios.CedulaAna))
                .ReturnsAsync(new List<SolicitudPromocion> { new() { RolObjetivo = "Agregado 2", Estado = SolicitudPromocion.Aprobada } });

            Assert.Equal(StatusCodes.Status409Conflict,
                await CodigoDeError(() => Servicio().CrearAsync(Usuarios.CedulaAna, Usuarios.Docente())));
            _solicitudes.Verify(r => r.CrearAsync(It.IsAny<SolicitudPromocion>()), Times.Never);
        }

        // ---------- Consultar ----------

        [Fact]
        public async Task Listar_DocenteSoloVeSusSolicitudes_RevisorVeTodas()
        {
            await Servicio().ListarAsync(null, Usuarios.Docente(cedula: Usuarios.CedulaAna));
            await Servicio().ListarAsync("Pendiente", Usuarios.Rrhh());

            _solicitudes.Verify(r => r.ListarAsync(null, Usuarios.CedulaAna));
            _solicitudes.Verify(r => r.ListarAsync("Pendiente", null));
        }

        [Fact]
        public async Task Obtener_SolicitudDeOtroDocente_Da403()
        {
            ConSolicitud(5, SolicitudPromocion.Pendiente, cedula: Usuarios.CedulaCarlos);

            Assert.Equal(StatusCodes.Status403Forbidden,
                await CodigoDeError(() => Servicio().ObtenerAsync(5, Usuarios.Docente(cedula: Usuarios.CedulaAna))));
        }

        // ---------- Revisar documentos ----------

        [Fact]
        public async Task RevisarDocumento_GuardaLaRevisionConElRevisor()
        {
            ConSolicitud(1, SolicitudPromocion.Pendiente);
            var request = new RevisarDocumentoRequest { TipoDocumento = "Obra", DocumentoId = 9, Estado = "Rechazado", Observacion = " PDF ilegible " };

            await Servicio().RevisarDocumentoAsync(1, request, Usuarios.Rrhh(id: 2));

            _solicitudes.Verify(r => r.GuardarRevisionAsync(It.Is<RevisionDocumento>(rv =>
                rv.SolicitudId == 1 && rv.TipoDocumento == "Obra" && rv.DocumentoId == 9 &&
                rv.Estado == "Rechazado" && rv.Observacion == "PDF ilegible" && rv.RevisadoPorId == 2)));
        }

        [Theory]
        [InlineData("Obra", "Rechazado", null)]        // rechazo sin motivo
        [InlineData("Obra", "Quizas", null)]           // estado inválido
        [InlineData("Diploma", "Aprobado", null)]      // tipo inválido
        public async Task RevisarDocumento_DatosInvalidos_Da400(string tipo, string estado, string? observacion)
        {
            ConSolicitud(1, SolicitudPromocion.Pendiente);
            var request = new RevisarDocumentoRequest { TipoDocumento = tipo, DocumentoId = 1, Estado = estado, Observacion = observacion };

            Assert.Equal(StatusCodes.Status400BadRequest,
                await CodigoDeError(() => Servicio().RevisarDocumentoAsync(1, request, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task RevisarDocumento_SolicitudYaResuelta_Da409()
        {
            ConSolicitud(1, SolicitudPromocion.Aprobada);
            var request = new RevisarDocumentoRequest { TipoDocumento = "Obra", DocumentoId = 1, Estado = "Aprobado" };

            Assert.Equal(StatusCodes.Status409Conflict,
                await CodigoDeError(() => Servicio().RevisarDocumentoAsync(1, request, Usuarios.Rrhh())));
        }

        // ---------- Resolver ----------

        [Fact]
        public async Task Aprobar_ConDocumentosRechazados_Da409()
        {
            ConSolicitud(1, SolicitudPromocion.Pendiente);
            _solicitudes.Setup(r => r.ObtenerRevisionesAsync(1)).ReturnsAsync(new List<RevisionDocumento>
            {
                new() { TipoDocumento = "Obra", DocumentoId = 1, Estado = RevisionDocumento.Rechazado }
            });

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() =>
                Servicio().ResolverAsync(1, new ResolverSolicitudRequest { Estado = "Aprobada" }, Usuarios.Rrhh())));
            _solicitudes.Verify(r => r.ResolverAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Rechazar_SinMotivo_Da400()
        {
            ConSolicitud(1, SolicitudPromocion.Pendiente);

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() =>
                Servicio().ResolverAsync(1, new ResolverSolicitudRequest { Estado = "Rechazada", Observacion = "  " }, Usuarios.Rrhh())));
        }

        [Fact]
        public async Task Aprobar_SinRechazos_RegistraQuienResolvio()
        {
            ConSolicitud(1, SolicitudPromocion.Pendiente);
            _solicitudes.Setup(r => r.ResolverAsync(1, "Aprobada", null, 1)).ReturnsAsync(true);

            await Servicio().ResolverAsync(1, new ResolverSolicitudRequest { Estado = "Aprobada" }, Usuarios.Administrador(id: 1));

            _solicitudes.Verify(r => r.ResolverAsync(1, "Aprobada", null, 1));
        }

        [Fact]
        public async Task Resolver_SiOtroRevisorSeAdelanto_Da409()
        {
            ConSolicitud(1, SolicitudPromocion.Pendiente);
            _solicitudes.Setup(r => r.ResolverAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() =>
                Servicio().ResolverAsync(1, new ResolverSolicitudRequest { Estado = "Aprobada" }, Usuarios.Rrhh())));
        }
    }
}
