using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<int> CrearAsync(SolicitudPromocion solicitud);
        Task<SolicitudPromocion?> ObtenerPorIdAsync(int id);

        // estado y cedula son filtros opcionales; soloEnviadasAComision limita a las que ya llegaron a la Comisión
        Task<List<SolicitudPromocion>> ListarAsync(string? estado, string? cedula, bool soloEnviadasAComision = false);

        // Hay una solicitud en revisión documental, en comisión o apelada para la cédula
        Task<bool> ExisteEnTramiteAsync(string cedula);

        Task<List<RevisionDocumento>> ObtenerRevisionesAsync(int solicitudId);
        Task GuardarRevisionAsync(RevisionDocumento revision);

        // Cada transición cambia el estado solo si la solicitud sigue en el estado esperado; devuelve false si otro usuario se adelantó
        Task<bool> EnviarAComisionAsync(int id, int usuarioId, string dictamenJson);
        Task<bool> ResolverAsync(int id, string estadoEsperado, string nuevoEstado, string numeroResolucion, string? observacion, int resueltoPorId);
    }
}
