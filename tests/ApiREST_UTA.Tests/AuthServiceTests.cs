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

        private static AuthService Servicio(Usuario? usuario, MiembroComision? miembro = null)
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.GetByCorreo(It.IsAny<string>())).Returns(usuario);
            var comision = new Mock<IComisionRepository>();
            comision.Setup(c => c.ObtenerMiembroActivoPorUsuarioAsync(It.IsAny<int>())).ReturnsAsync(miembro);
            return new AuthService(repo.Object, comision.Object, ClaveJwt);
        }

        [Fact]
        public async Task CredencialesValidas_DevuelveTokenConRolYCedula()
        {
            var respuesta = await Servicio(Ana()).AutenticarAsync(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = Contrasena });

            Assert.NotNull(respuesta);
            Assert.Equal(3, respuesta!.Id);
            Assert.Equal(Usuarios.CedulaAna, respuesta.Cedula);
            Assert.Equal("Ana Pérez", respuesta.NombreCompleto);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(respuesta.Token);
            Assert.Contains(token.Claims, c => c.Type == ClaimsUsuario.Cedula && c.Value == Usuarios.CedulaAna);
            Assert.Contains(token.Claims, c => c.Value == RolesSistema.Docente);
            Assert.Equal("ApiREST_UTA", token.Issuer);
            Assert.False(respuesta.EsMiembroComision);
            Assert.DoesNotContain(token.Claims, c => c.Type == ClaimsUsuario.Comision);
        }

        [Fact]
        public async Task MiembroDeLaComision_LlevaElCargoEnLaRespuestaYEnElToken()
        {
            var miembro = new MiembroComision { Id = 3, UsuarioId = 3, Cargo = MiembroComision.ProfesorTerna };

            var respuesta = await Servicio(Ana(), miembro).AutenticarAsync(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = Contrasena });

            Assert.NotNull(respuesta);
            Assert.True(respuesta!.EsMiembroComision);
            Assert.False(respuesta.EsPresidenteComision);
            Assert.Equal(MiembroComision.ProfesorTerna, respuesta.CargoComision);
            var token = new JwtSecurityTokenHandler().ReadJwtToken(respuesta.Token);
            Assert.Contains(token.Claims, c => c.Type == ClaimsUsuario.Comision && c.Value == "true");
        }

        [Fact]
        public async Task ContrasenaIncorrecta_DevuelveNull()
        {
            var respuesta = await Servicio(Ana()).AutenticarAsync(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = "otra" });

            Assert.Null(respuesta);
        }

        [Fact]
        public async Task UsuarioDesactivado_NoPuedeIniciarSesion()
        {
            var respuesta = await Servicio(Ana(activo: false)).AutenticarAsync(new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = Contrasena });

            Assert.Null(respuesta);
        }

        [Fact]
        public async Task CorreoInexistente_DevuelveNull()
        {
            var respuesta = await Servicio(null).AutenticarAsync(new LoginRequest { Correo = "nadie@uta.edu.ec", Contrasena = Contrasena });

            Assert.Null(respuesta);
        }
    }
}
