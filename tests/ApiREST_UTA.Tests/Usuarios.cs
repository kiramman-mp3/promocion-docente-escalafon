using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using System.Security.Claims;

namespace ApiREST_UTA.Tests
{
    // Construye el ClaimsPrincipal que la API obtiene del JWT para cada tipo de usuario
    public static class Usuarios
    {
        public const string CedulaAna = "1801234567";
        public const string CedulaCarlos = "1809876543";

        public static ClaimsPrincipal Docente(int id = 3, string? cedula = CedulaAna) =>
            Crear(id, RolesSistema.Docente, cedula);

        public static ClaimsPrincipal Rrhh(int id = 2) => Crear(id, RolesSistema.RRHH, null);

        public static ClaimsPrincipal Administrador(int id = 1) => Crear(id, RolesSistema.Administrador, null);

        private static ClaimsPrincipal Crear(int id, string rol, string? cedula)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id.ToString()),
                new(ClaimTypes.Role, rol)
            };
            if (cedula != null)
                claims.Add(new Claim(ClaimsUsuario.Cedula, cedula));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Pruebas"));
        }
    }
}
