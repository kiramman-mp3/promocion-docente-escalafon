using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IRRHHRepository
    {
        Task<TiempoRolResponse?> ObtenerTiempoRolPorCedulaAsync(string cedula);
        Task<TrayectoriaDocente?> ObtenerTrayectoriaAsync(string cedula);
        Task<List<CargoAutoridad>> ObtenerCargosAutoridadAsync(string cedula);
        Task<List<Sabatico>> ObtenerSabaticosAsync(string cedula);
    }
}
