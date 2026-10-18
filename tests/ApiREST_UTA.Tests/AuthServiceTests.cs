using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace ApiREST_UTA.Tests
{
    public class AuthServiceTests
    {
        private const string ClaveJwt = "clave-de-pruebas-con-mas-de-treinta-y-dos-caracteres";
        private const string Contrasena = "Uta2026*";

        private static Usuario Ana(bool activo = true) => new()
        {
            Id = 3,
            Correo = "ana.perez@uta.edu.ec",
            Contrasena = BCrypt.Net.BCrypt.HashPassword(Contrasena, 4),
            Nombre = "Ana",
            Apellido = "Pérez",
            Rol = RolesSistema.Docente,
            Cedula = Usuarios.CedulaAna,
            Activo = activo
        };

        private static AuthService Servicio(Usuario? usuario)
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.GetByCorreo(It.IsAny<string>())).Returns(usuario);
            return new AuthService(repo.Object, ClaveJwt);
        }

        [Fact]
        public void CredencialesValidas_DevuelveTokenConRolYCedula()
        {
            var respuesta = Servicio(Ana()).Autenticar(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = Contrasena });

            Assert.NotNull(respuesta);
            Assert.Equal(3, respuesta!.Id);
            Assert.Equal(Usuarios.CedulaAna, respuesta.Cedula);
            Assert.Equal("Ana Pérez", respuesta.NombreCompleto);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(respuesta.Token);
            Assert.Contains(token.Claims, c => c.Type == ClaimsUsuario.Cedula && c.Value == Usuarios.CedulaAna);
            Assert.Contains(token.Claims, c => c.Value == RolesSistema.Docente);
            Assert.Equal("ApiREST_UTA", token.Issuer);
        }

        [Fact]
        public void ContrasenaIncorrecta_DevuelveNull()
        {
            var respuesta = Servicio(Ana()).Autenticar(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = "otra" });

            Assert.Null(respuesta);
        }

        [Fact]
        public void UsuarioDesactivado_NoPuedeIniciarSesion()
        {
            var respuesta = Servicio(Ana(activo: false)).Autenticar(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = Contrasena });

            Assert.Null(respuesta);
        }

        [Fact]
        public void CorreoInexistente_DevuelveNull()
        {
            var respuesta = Servicio(null).Autenticar(new LoginRequest { Correo = "nadie@uta.edu.ec", Contrasena = Contrasena });

            Assert.Null(respuesta);
        }
    }
}
