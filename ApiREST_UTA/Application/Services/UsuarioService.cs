using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ApiREST_UTA.Application.Services
{
    // Administración de cuentas: solo la usa el rol Administrador.
    public class UsuarioService
    {
        private const int LongitudMinimaContrasena = 8;
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<UsuarioResponse>> ListarAsync()
        {
            return (await _repo.ListarAsync()).Select(AResponse).ToList();
        }

        public async Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request)
        {
            var correo = request.Correo.Trim().ToLowerInvariant();
            if (!MailAddress.TryCreate(correo, out _))
                throw ReglaNegocioException.Invalido("El correo no es válido.");
            if (_repo.GetByCorreo(correo) != null)
                throw ReglaNegocioException.Conflicto("Ya existe un usuario con ese correo.");
            ValidarContrasena(request.Contrasena);

            var usuario = new Usuario
            {
                Correo = correo,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
                Activo = true
            };
            AplicarDatos(usuario, request.Nombre, request.Apellido, request.Rol, request.Cedula);

            usuario.Id = await _repo.CrearAsync(usuario);
            return AResponse(usuario);
        }

        public async Task<UsuarioResponse> ActualizarAsync(int id, ActualizarUsuarioRequest request, ClaimsPrincipal admin)
        {
            var usuario = await _repo.ObtenerPorIdAsync(id)
                ?? throw ReglaNegocioException.NoEncontrado("El usuario no existe.");

            // Evita que el administrador se quede sin acceso por error
            if (id == admin.Id() && (!request.Activo || request.Rol != RolesSistema.Administrador))
                throw ReglaNegocioException.Invalido("No puedes desactivarte ni quitarte el rol de Administrador.");

            AplicarDatos(usuario, request.Nombre, request.Apellido, request.Rol, request.Cedula);
            usuario.Activo = request.Activo;

            await _repo.ActualizarAsync(usuario);
            return AResponse(usuario);
        }

        public async Task CambiarContrasenaAsync(int id, CambiarContrasenaRequest request)
        {
            if (await _repo.ObtenerPorIdAsync(id) == null)
                throw ReglaNegocioException.NoEncontrado("El usuario no existe.");
            ValidarContrasena(request.Contrasena);

            await _repo.CambiarContrasenaAsync(id, BCrypt.Net.BCrypt.HashPassword(request.Contrasena));
        }

        private static void AplicarDatos(Usuario usuario, string nombre, string apellido, string rol, string? cedula)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
                throw ReglaNegocioException.Invalido("Nombre y apellido son obligatorios.");
            if (!RolesSistema.Todos.Contains(rol))
                throw ReglaNegocioException.Invalido($"Rol inválido. Usa: {string.Join(", ", RolesSistema.Todos)}.");

            cedula = string.IsNullOrWhiteSpace(cedula) ? null : cedula.Trim();
            if (rol == RolesSistema.Docente && cedula == null)
                throw ReglaNegocioException.Invalido("Un docente necesita su cédula para ver su expediente.");
            if (cedula != null && !Regex.IsMatch(cedula, @"^\d{10}$"))
                throw ReglaNegocioException.Invalido("La cédula debe tener 10 dígitos.");

            usuario.Nombre = nombre.Trim();
            usuario.Apellido = apellido.Trim();
            usuario.Rol = rol;
            usuario.Cedula = cedula;
        }

        private static void ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrEmpty(contrasena) || contrasena.Length < LongitudMinimaContrasena)
                throw ReglaNegocioException.Invalido($"La contraseña debe tener al menos {LongitudMinimaContrasena} caracteres.");
        }

        private static UsuarioResponse AResponse(Usuario u) => new()
        {
            Id = u.Id,
            Correo = u.Correo,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Rol = u.Rol,
            Cedula = u.Cedula,
            Activo = u.Activo
        };
    }
}
