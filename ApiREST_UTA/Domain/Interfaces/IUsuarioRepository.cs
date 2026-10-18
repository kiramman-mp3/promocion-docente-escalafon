using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Usuario? GetByCorreo(string correo);
        Task<List<Usuario>> ListarAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario); // Nombre, Apellido, Rol, Cedula y Activo
        Task CambiarContrasenaAsync(int id, string contrasenaHash);
    }
}
