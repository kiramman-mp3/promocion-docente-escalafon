using ApiREST_UTA.Application.DTOs;

namespace ApiREST_UTA.Domain.Interfaces
{
    namespace ApiREST_UTA.Domain.Interfaces
    {
        public interface IRRHHRepository
        {
            Task<TiempoRolResponse?> ObtenerTiempoRolPorCedulaAsync(string cedula);
        }
    }
}
