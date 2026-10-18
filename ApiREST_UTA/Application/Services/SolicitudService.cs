using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using System.Security.Claims;

namespace ApiREST_UTA.Application.Services
{
    // Flujo de aprobación: el docente (o RRHH) crea la solicitud, RRHH/Administrador revisa
    // cada documento del expediente y luego aprueba o rechaza la solicitud.
    public class SolicitudService
    {
        private readonly ISolicitudRepository _solicitudRepo;
        private readonly IRRHHRepository _rrhhRepo;
        private readonly IRolRepository _rolRepo;

        public SolicitudService(ISolicitudRepository solicitudRepo, IRRHHRepository rrhhRepo, IRolRepository rolRepo)
        {
            _solicitudRepo = solicitudRepo;
            _rrhhRepo = rrhhRepo;
            _rolRepo = rolRepo;
        }

        public async Task<SolicitudResponse> CrearAsync(string cedula, ClaimsPrincipal usuario)
        {
            cedula = cedula.Trim();
            var tiempo = await _rrhhRepo.ObtenerTiempoRolPorCedulaAsync(cedula)
                ?? throw ReglaNegocioException.NoEncontrado($"RRHH no tiene registrada la cédula {cedula}.");

            var rolActual = await _rolRepo.ObtenerPorNombre(tiempo.RolActual)
                ?? throw ReglaNegocioException.Invalido($"El rol \"{tiempo.RolActual}\" no está en el escalafón.");
            var rolObjetivo = await _rolRepo.BuscarSiguienteRol(rolActual.Id)
                ?? throw ReglaNegocioException.Invalido($"{rolActual.Nombre} es el último rol del escalafón.");

            if (await _solicitudRepo.ExistePendienteAsync(cedula))
                throw ReglaNegocioException.Conflicto("Ya hay una solicitud pendiente para esta cédula.");

            // RRHH es una base externa: tras aprobar, el rol actual no cambia hasta que RRHH lo actualice
            var aprobadas = await _solicitudRepo.ListarAsync(SolicitudPromocion.Aprobada, cedula);
            if (aprobadas.Any(s => s.RolObjetivo == rolObjetivo.Nombre))
                throw ReglaNegocioException.Conflicto(
                    $"La promoción a {rolObjetivo.Nombre} ya fue aprobada; falta que RRHH registre el nuevo rol.");

            var id = await _solicitudRepo.CrearAsync(new SolicitudPromocion
            {
                Cedula = cedula,
                RolActualId = rolActual.Id,
                RolActual = rolActual.Nombre,
                RolObjetivo = rolObjetivo.Nombre,
                SolicitadoPorId = usuario.Id()
            });

            return (await ObtenerAsync(id, usuario))!;
        }

        public async Task<List<SolicitudResponse>> ListarAsync(string? estado, ClaimsPrincipal usuario)
        {
            // Un docente solo ve sus propias solicitudes
            var cedula = usuario.EsRevisor() ? null : usuario.CedulaPropia() ?? "";
            var solicitudes = await _solicitudRepo.ListarAsync(string.IsNullOrWhiteSpace(estado) ? null : estado, cedula);
            return solicitudes.Select(s => AResponse(s, null)).ToList();
        }

        public async Task<SolicitudResponse?> ObtenerAsync(int id, ClaimsPrincipal usuario)
        {
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id);
            if (solicitud == null) return null;
            if (!usuario.PuedeConsultar(solicitud.Cedula))
                throw ReglaNegocioException.Prohibido("Solo puedes ver tus propias solicitudes.");

            var revisiones = await _solicitudRepo.ObtenerRevisionesAsync(id);
            return AResponse(solicitud, revisiones);
        }

        public async Task<SolicitudResponse> RevisarDocumentoAsync(int id, RevisarDocumentoRequest request, ClaimsPrincipal usuario)
        {
            await ObtenerPendienteAsync(id);

            if (!RevisionDocumento.Tipos.Contains(request.TipoDocumento))
                throw ReglaNegocioException.Invalido($"Tipo de documento inválido. Usa: {string.Join(", ", RevisionDocumento.Tipos)}.");
            if (request.Estado != RevisionDocumento.Aprobado && request.Estado != RevisionDocumento.Rechazado)
                throw ReglaNegocioException.Invalido("El estado debe ser Aprobado o Rechazado.");
            if (request.Estado == RevisionDocumento.Rechazado && string.IsNullOrWhiteSpace(request.Observacion))
                throw ReglaNegocioException.Invalido("Indica el motivo del rechazo del documento.");

            await _solicitudRepo.GuardarRevisionAsync(new RevisionDocumento
            {
                SolicitudId = id,
                TipoDocumento = request.TipoDocumento,
                DocumentoId = request.DocumentoId,
                Estado = request.Estado,
                Observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim(),
                RevisadoPorId = usuario.Id()
            });

            return (await ObtenerAsync(id, usuario))!;
        }

        public async Task<SolicitudResponse> ResolverAsync(int id, ResolverSolicitudRequest request, ClaimsPrincipal usuario)
        {
            await ObtenerPendienteAsync(id);

            if (request.Estado != SolicitudPromocion.Aprobada && request.Estado != SolicitudPromocion.Rechazada)
                throw ReglaNegocioException.Invalido("El estado debe ser Aprobada o Rechazada.");
            if (request.Estado == SolicitudPromocion.Rechazada && string.IsNullOrWhiteSpace(request.Observacion))
                throw ReglaNegocioException.Invalido("Indica el motivo del rechazo de la solicitud.");

            if (request.Estado == SolicitudPromocion.Aprobada)
            {
                var revisiones = await _solicitudRepo.ObtenerRevisionesAsync(id);
                if (revisiones.Any(r => r.Estado == RevisionDocumento.Rechazado))
                    throw ReglaNegocioException.Conflicto("No se puede aprobar: hay documentos rechazados en el expediente.");
            }

            var observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim();
            if (!await _solicitudRepo.ResolverAsync(id, request.Estado, observacion, usuario.Id()))
                throw ReglaNegocioException.Conflicto("La solicitud ya fue resuelta por otra persona.");

            return (await ObtenerAsync(id, usuario))!;
        }

        private async Task<SolicitudPromocion> ObtenerPendienteAsync(int id)
        {
            var solicitud = await _solicitudRepo.ObtenerPorIdAsync(id)
                ?? throw ReglaNegocioException.NoEncontrado("La solicitud no existe.");
            if (solicitud.Estado != SolicitudPromocion.Pendiente)
                throw ReglaNegocioException.Conflicto($"La solicitud ya está {solicitud.Estado.ToLower()}.");
            return solicitud;
        }

        private static SolicitudResponse AResponse(SolicitudPromocion s, List<RevisionDocumento>? revisiones) => new()
        {
            Id = s.Id,
            Cedula = s.Cedula,
            RolActualId = s.RolActualId,
            RolActual = s.RolActual,
            RolObjetivo = s.RolObjetivo,
            Estado = s.Estado,
            SolicitadoPor = s.SolicitadoPor,
            FechaSolicitud = s.FechaSolicitud,
            ResueltoPor = s.ResueltoPor,
            FechaResolucion = s.FechaResolucion,
            Observacion = s.Observacion,
            Revisiones = revisiones?.Select(r => new RevisionDocumentoResponse
            {
                TipoDocumento = r.TipoDocumento,
                DocumentoId = r.DocumentoId,
                Estado = r.Estado,
                Observacion = r.Observacion,
                RevisadoPor = r.RevisadoPor,
                FechaRevision = r.FechaRevision
            }).ToList()
        };
    }
}
