using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IProyectoPonderadoRepository
    {
        Task<int> ObtenerMesesPonderadosAsync(string cedula);
        Task<List<ProyectoParticipacion>> ObtenerProyectosAsync(string cedula);
    }
}
