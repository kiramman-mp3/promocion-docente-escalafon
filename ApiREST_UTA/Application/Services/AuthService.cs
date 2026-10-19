using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiREST_UTA.Application.Services
{
    public class AuthService
    {
        private readonly IUsuarioRepository _repo;
        private readonly IComisionRepository _comision;
        private readonly string _jwtKey;

        public AuthService(IUsuarioRepository repo, IComisionRepository comision, string jwtKey)
        {
            _repo = repo;
            _comision = comision;
            _jwtKey = jwtKey;
        }

        public async Task<LoginResponse?> AutenticarAsync(LoginRequest request)
        {
            var usuario = _repo.GetByCorreo(request.Correo);

            if (usuario == null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.Contrasena))
                return null;

            // La pertenencia a la Comisión Académica viaja en el token (UTA art. 47)
            var miembro = await _comision.ObtenerMiembroActivoPorUsuarioAsync(usuario.Id);
            var token = GenerarToken(usuario, miembro != null);
            return new LoginResponse
            {
                Token = token,
                Id = usuario.Id,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Cedula = usuario.Cedula,
                EsMiembroComision = miembro != null,
                EsPresidenteComision = miembro?.EsPresidente ?? false,
                CargoComision = miembro?.Cargo
            };
        }

        private string GenerarToken(Usuario usuario, bool miembroComision)
        {
            var claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
               new Claim(ClaimTypes.Name, usuario.Correo),
               new Claim(ClaimTypes.Role, usuario.Rol)
            };
            if (!string.IsNullOrWhiteSpace(usuario.Cedula))
                claims.Add(new Claim(ClaimsUsuario.Cedula, usuario.Cedula));
            if (miembroComision)
                claims.Add(new Claim(ClaimsUsuario.Comision, "true"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "ApiREST_UTA",
                audience: "ApiREST_UTA",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
