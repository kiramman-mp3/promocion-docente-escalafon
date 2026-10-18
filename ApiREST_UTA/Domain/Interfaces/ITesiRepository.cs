using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface ITesisRepository
    {
        // Número de tesis doctorales dirigidas (se mantiene para /api/importar/tesis-doctorado)
        Task<int> ObtenerTesisPorCedulaAsync(string cedula);
        Task<List<TrabajoTitulacion>> ObtenerTrabajosTitulacionAsync(string cedula);
    }
}
