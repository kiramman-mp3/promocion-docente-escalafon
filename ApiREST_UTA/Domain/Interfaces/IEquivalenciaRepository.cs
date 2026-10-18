using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IEquivalenciaRepository
    {
        Task<List<Equivalencia>> ObtenerEquivalenciasAsync(string cedula);
    }
}
