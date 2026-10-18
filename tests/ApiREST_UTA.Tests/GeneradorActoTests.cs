using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Application.Services.Promocion;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Infrastructure.Documentos;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiREST_UTA.Tests
{
    // CES art. 77 l: la promoción consta en un acto administrativo con la fecha en que se llevó a cabo
    public class GeneradorActoTests
    {
        private static DatosActo Datos(string estado) => new()
        {
            Solicitud = new SolicitudPromocion
            {
                Id = 12,
                Cedula = Usuarios.CedulaAna,
                RolActual = "Agregado 1",
                RolObjetivo = "Agregado 2",
                Estado = estado,
                NumeroResolucion = "CAP-2026-0012",
                FechaSolicitud = new DateTime(2026, 9, 1, 15, 0, 0, DateTimeKind.Utc),
                FechaEnvioComision = new DateTime(2026, 9, 20, 15, 0, 0, DateTimeKind.Utc),
                FechaResolucion = new DateTime(2026, 10, 1, 15, 0, 0, DateTimeKind.Utc),
                Observacion = "Cumple todos los requisitos."
            },
            NombreDocente = "Ana María Pérez Villacís",
            Dictamen = new DictamenPromocion
            {
                Normativa = "CES art. 76; UTA art. 48",
                RequisitosPropuestos = true,
                Requisitos = { new ResultadoRequisito { Codigo = "R2", Nombre = "Tiempo en el grado actual", Articulo = "CES art. 76 a", Cumple = true, Obtenido = "6 años", Requerido = "4 años" } }
            },
            Votos = { new VotoComision { Miembro = "Fernando Castillo", Cargo = MiembroComision.VicerrectorAcademico, EsPresidente = true, Voto = VotoComision.AFavor, Motivacion = "Cumple." } },
            Presidente = "Fernando Castillo"
        };

        private static string Texto(byte[] pdf) => Encoding.Latin1.GetString(pdf);

        [Fact]
        public void GeneraUnPdfValidoConTablaDeReferenciasCorrecta()
        {
            var pdf = new GeneradorActo().Generar(Datos(SolicitudPromocion.Aprobada));
            var texto = Texto(pdf);

            Assert.StartsWith("%PDF-1.4", texto);
            Assert.EndsWith("%%EOF\n", texto);

            // Cada entrada del xref apunta al inicio de su objeto
            var inicioXref = int.Parse(Regex.Match(texto, @"startxref\n(\d+)").Groups[1].Value);
            Assert.StartsWith("xref", texto[inicioXref..]);
            var entradas = Regex.Matches(texto[inicioXref..], @"(\d{10}) 00000 n").Select(m => int.Parse(m.Groups[1].Value)).ToList();
            for (var i = 0; i < entradas.Count; i++)
                Assert.StartsWith($"{i + 1} 0 obj", texto[entradas[i]..]);
        }

        [Fact]
        public void IncluyeNumeroDecisionArticulosYVotos()
        {
            var texto = Texto(new GeneradorActo().Generar(Datos(SolicitudPromocion.Aprobada)));

            Assert.Contains("CAP-2026-0012", texto);
            Assert.Contains("APROBAR la promoci", texto);
            Assert.Contains("77 literal l", texto);
            Assert.Contains("Fernando Castillo", texto);
            Assert.Contains("Ana María Pérez", texto);
        }

        [Fact]
        public void CuandoSeNiega_InformaElRecursoDeApelacion()
        {
            var texto = Texto(new GeneradorActo().Generar(Datos(SolicitudPromocion.Rechazada)));

            Assert.Contains("NEGAR la promoci", texto);
            Assert.Contains("recurso de apelaci", texto);
        }
    }
}
