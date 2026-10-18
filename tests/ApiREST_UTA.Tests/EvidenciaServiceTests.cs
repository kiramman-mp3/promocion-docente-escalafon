using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace ApiREST_UTA.Tests
{
    // CES art. 99: el docente anexa a su solicitud las evidencias de que cumple los requisitos
    public class EvidenciaServiceTests
    {
        private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.4\n% evidencia de prueba\n%%EOF");

        private readonly Mock<IEvidenciaRepository> _evidencias = new();
        private readonly Mock<ISolicitudRepository> _solicitudes = new();

        private EvidenciaService Servicio(string estado = SolicitudPromocion.RevisionDocumental, string cedula = Usuarios.CedulaAna)
        {
            _solicitudes.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(new SolicitudPromocion
            {
                Id = 1,
                Cedula = cedula,
                RolActual = "Agregado 1",
                RolObjetivo = "Agregado 2",
                Estado = estado
            });
            _evidencias.Setup(r => r.CrearAsync(It.IsAny<EvidenciaSolicitud>())).ReturnsAsync(7);
            return new EvidenciaService(_evidencias.Object, _solicitudes.Object);
        }

        private static async Task<int> CodigoDeError(Func<Task> accion) =>
            (await Assert.ThrowsAsync<ReglaNegocioException>(accion)).StatusCode;

        [Fact]
        public async Task Art99_ElDocenteAnexaUnPdfYSeGuardaSuHash()
        {
            var evidencia = await Servicio().SubirAsync(1, "CertificadoCongreso", "Congreso CITIS", "certificado.pdf", Pdf, Usuarios.Docente());

            Assert.Equal(7, evidencia.Id);
            Assert.Equal(64, evidencia.Hash.Length);
            _evidencias.Verify(r => r.CrearAsync(It.Is<EvidenciaSolicitud>(e => e.SolicitudId == 1 && e.Tamano == Pdf.Length)));
        }

        [Fact]
        public async Task SoloSeAceptanPdfReales_NoBastaLaExtension()
        {
            var falso = Encoding.ASCII.GetBytes("MZ ejecutable disfrazado");

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() =>
                Servicio().SubirAsync(1, "Otro", "x", "virus.pdf", falso, Usuarios.Docente())));
        }

        [Fact]
        public async Task ArchivoDeMasDe10MB_Da400()
        {
            var grande = new byte[EvidenciaSolicitud.TamanoMaximo + 1];
            Pdf.CopyTo(grande, 0);

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() =>
                Servicio().SubirAsync(1, "Otro", "x", "grande.pdf", grande, Usuarios.Docente())));
        }

        [Fact]
        public async Task OtroDocente_NoPuedeAnexarEvidencias()
        {
            Assert.Equal(StatusCodes.Status403Forbidden, await CodigoDeError(() =>
                Servicio(cedula: Usuarios.CedulaCarlos).SubirAsync(1, "Otro", "x", "a.pdf", Pdf, Usuarios.Docente())));
        }

        [Fact]
        public async Task FueraDeLaRevisionDocumental_Da409()
        {
            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() =>
                Servicio(estado: SolicitudPromocion.EnComision).SubirAsync(1, "Otro", "x", "a.pdf", Pdf, Usuarios.Docente())));
        }

        [Fact]
        public async Task TipoInexistente_Da400()
        {
            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() =>
                Servicio().SubirAsync(1, "Diploma", "x", "a.pdf", Pdf, Usuarios.Docente())));
        }
    }
}
