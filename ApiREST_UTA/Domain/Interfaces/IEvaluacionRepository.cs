using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IEvaluacionDocenteRepository
    {
        Task<List<EvaluacionDocente>> ObtenerEvaluacionesPorCedula(string cedula);
    }
}
