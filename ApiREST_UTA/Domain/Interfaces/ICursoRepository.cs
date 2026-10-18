using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface ICursoRepository
    {
        Task<IEnumerable<CursoCapacitacion>> ObtenerCursosPorCedula(string cedula);
    }
}
