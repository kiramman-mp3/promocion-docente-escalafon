using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<int> CrearAsync(SolicitudPromocion solicitud);
        Task<SolicitudPromocion?> ObtenerPorIdAsync(int id);
        Task<List<SolicitudPromocion>> ListarAsync(string? estado, string? cedula);
        Task<bool> ExistePendienteAsync(string cedula);
        Task<List<RevisionDocumento>> ObtenerRevisionesAsync(int solicitudId);
        Task GuardarRevisionAsync(RevisionDocumento revision);
        // Devuelve false si la solicitud ya no estaba pendiente
        Task<bool> ResolverAsync(int id, string estado, string? observacion, int resueltoPorId);
    }
}
