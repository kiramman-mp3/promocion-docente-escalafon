using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IRolRepository
    {
        Task<Rol> ObtenerSiguienteRol(int rolActualId);
        Task<Rol?> BuscarSiguienteRol(int rolActualId); // null si ya es el último rol
        Task<Rol?> ObtenerPorNombre(string nombre);
        Task<Rol?> ObtenerPorIdAsync(int id);
        Task<List<Rol>> ListarAsync();
    }
}
