using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IApelacionRepository
    {
        Task<Apelacion?> ObtenerPorSolicitudAsync(int solicitudId);

        // Registra la apelación y pasa la solicitud de Rechazada a Apelada en una transacción
        Task<bool> PresentarAsync(Apelacion apelacion);

        Task<bool> ResolverAsync(int solicitudId, string estado, string resolucionHCU, string motivacion, int resueltaPorId);
    }
}
