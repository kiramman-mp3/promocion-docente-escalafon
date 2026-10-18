using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IEvidenciaRepository
    {
        Task<int> CrearAsync(EvidenciaSolicitud evidencia);

        // Sin el contenido del archivo
        Task<List<EvidenciaSolicitud>> ListarAsync(int solicitudId);

        // Con el contenido del archivo
        Task<EvidenciaSolicitud?> ObtenerAsync(int solicitudId, int evidenciaId);
    }
}
