using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ApiREST_UTA.Application.Services
{
    public class EvidenciaResponse
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public string Tipo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public int Tamano { get; set; }
        public string? SubidoPor { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    // Evidencias que el docente anexa a su solicitud (CES art. 99)
    public class EvidenciaService
    {
        private readonly IEvidenciaRepository _evidencias;
        private readonly ISolicitudRepository _solicitudes;

        public EvidenciaService(IEvidenciaRepository evidencias, ISolicitudRepository solicitudes)
        {
            _evidencias = evidencias;
            _solicitudes = solicitudes;
        }

        public async Task<List<EvidenciaResponse>> ListarAsync(int solicitudId, ClaimsPrincipal usuario)
        {
            await ObtenerSolicitudVisibleAsync(solicitudId, usuario);
            return (await _evidencias.ListarAsync(solicitudId)).Select(AResponse).ToList();
        }

        public async Task<EvidenciaSolicitud> DescargarAsync(int solicitudId, int evidenciaId, ClaimsPrincipal usuario)
        {
            await ObtenerSolicitudVisibleAsync(solicitudId, usuario);
            return await _evidencias.ObtenerAsync(solicitudId, evidenciaId)
                ?? throw ReglaNegocioException.NoEncontrado("La evidencia no existe.");
        }

        public async Task<EvidenciaResponse> SubirAsync(int solicitudId, string tipo, string descripcion, string nombreArchivo,
            byte[] contenido, ClaimsPrincipal usuario)
        {
            var solicitud = await ObtenerSolicitudVisibleAsync(solicitudId, usuario);
            if (!usuario.EsRevisor() && usuario.CedulaPropia() != solicitud.Cedula)
                throw ReglaNegocioException.Prohibido("Solo el docente de la solicitud o Talento Humano pueden anexar evidencias.");
            if (!solicitud.AdmiteEvidencias)
                throw ReglaNegocioException.Conflicto("Solo se pueden anexar evidencias mientras la solicitud está en revisión documental.");
            if (!EvidenciaSolicitud.Tipos.Contains(tipo))
                throw ReglaNegocioException.Invalido($"Tipo de evidencia inválido. Usa: {string.Join(", ", EvidenciaSolicitud.Tipos)}.");
            if (string.IsNullOrWhiteSpace(descripcion))
                throw ReglaNegocioException.Invalido("Describe qué demuestra la evidencia.");
            if (contenido.Length == 0 || contenido.Length > EvidenciaSolicitud.TamanoMaximo)
                throw ReglaNegocioException.Invalido("El archivo debe pesar entre 1 byte y 10 MB.");
            if (!EvidenciaSolicitud.EsPdf(contenido))
                throw ReglaNegocioException.Invalido("Solo se aceptan archivos PDF.");

            var evidencia = new EvidenciaSolicitud
            {
                SolicitudId = solicitudId,
                Tipo = tipo,
                Descripcion = descripcion.Trim(),
                NombreArchivo = Path.GetFileName(nombreArchivo).Trim() is { Length: > 0 } n ? n[..Math.Min(n.Length, 200)] : "evidencia.pdf",
                Contenido = contenido,
                Hash = Convert.ToHexString(SHA256.HashData(contenido)).ToLowerInvariant(),
                Tamano = contenido.Length,
                SubidoPorId = usuario.Id(),
                FechaSubida = DateTime.UtcNow
            };
            evidencia.Id = await _evidencias.CrearAsync(evidencia);
            return AResponse(evidencia);
        }

        private async Task<SolicitudPromocion> ObtenerSolicitudVisibleAsync(int solicitudId, ClaimsPrincipal usuario)
        {
            var solicitud = await _solicitudes.ObtenerPorIdAsync(solicitudId)
                ?? throw ReglaNegocioException.NoEncontrado("La solicitud no existe.");
            if (!usuario.PuedeVerSolicitud(solicitud))
                throw ReglaNegocioException.Prohibido("No tienes acceso a esta solicitud.");
            return solicitud;
        }

        private static EvidenciaResponse AResponse(EvidenciaSolicitud e) => new()
        {
            Id = e.Id,
            SolicitudId = e.SolicitudId,
            Tipo = e.Tipo,
            Descripcion = e.Descripcion,
            NombreArchivo = e.NombreArchivo,
            Hash = e.Hash,
            Tamano = e.Tamano,
            SubidoPor = e.SubidoPor,
            FechaSubida = e.FechaSubida
        };
    }
}
