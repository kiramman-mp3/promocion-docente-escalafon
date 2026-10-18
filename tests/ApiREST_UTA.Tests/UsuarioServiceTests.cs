using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ApiREST_UTA.Tests
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _repo = new();

        private UsuarioService Servicio() => new(_repo.Object);

        private static CrearUsuarioRequest NuevoDocente() => new()
        {
            Correo = "  Maria.Lopez@UTA.edu.ec ",
            Nombre = " María ",
            Apellido = "López",
            Rol = RolesSistema.Docente,
            Cedula = "1700000001",
            Contrasena = "Temporal2026"
        };

        private static async Task<int> CodigoDeError(Func<Task> accion) =>
            (await Assert.ThrowsAsync<ReglaNegocioException>(accion)).StatusCode;

        [Fact]
        public async Task Crear_NormalizaElCorreoYGuardaLaContrasenaConBCrypt()
        {
            Usuario? guardado = null;
            _repo.Setup(r => r.CrearAsync(It.IsAny<Usuario>())).Callback<Usuario>(u => guardado = u).ReturnsAsync(12);

            var creado = await Servicio().CrearAsync(NuevoDocente());

            Assert.Equal(12, creado.Id);
            Assert.Equal("maria.lopez@uta.edu.ec", creado.Correo);
            Assert.Equal("María", creado.Nombre);
            Assert.True(creado.Activo);
            Assert.NotEqual("Temporal2026", guardado!.Contrasena);
            Assert.True(BCrypt.Net.BCrypt.Verify("Temporal2026", guardado.Contrasena));
        }

        [Fact]
        public async Task Crear_CorreoDuplicado_Da409()
        {
            _repo.Setup(r => r.GetByCorreo("maria.lopez@uta.edu.ec")).Returns(new Usuario());

            Assert.Equal(StatusCodes.Status409Conflict, await CodigoDeError(() => Servicio().CrearAsync(NuevoDocente())));
        }

        [Theory]
        [InlineData("no-es-un-correo", "Temporal2026", "Docente", "1700000001")]
        [InlineData("a@uta.edu.ec", "corta", "Docente", "1700000001")]
        [InlineData("a@uta.edu.ec", "Temporal2026", "Docente", null)]           // docente sin cédula
        [InlineData("a@uta.edu.ec", "Temporal2026", "Docente", "17000A0001")]   // cédula con letras
        [InlineData("a@uta.edu.ec", "Temporal2026", "Rector", null)]           // rol inexistente
        public async Task Crear_DatosInvalidos_Da400(string correo, string contrasena, string rol, string? cedula)
        {
            var request = NuevoDocente();
            request.Correo = correo;
            request.Contrasena = contrasena;
            request.Rol = rol;
            request.Cedula = cedula;

            Assert.Equal(StatusCodes.Status400BadRequest, await CodigoDeError(() => Servicio().CrearAsync(request)));
            _repo.Verify(r => r.CrearAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Crear_RrhhNoNecesitaCedula()
        {
            var request = NuevoDocente();
            request.Rol = RolesSistema.RRHH;
            request.Cedula = null;

            var creado = await Servicio().CrearAsync(request);

            Assert.Equal(RolesSistema.RRHH, creado.Rol);
            Assert.Null(creado.Cedula);
        }

        [Theory]
        [InlineData(false, "Administrador")]
        [InlineData(true, "RRHH")]
        public async Task Actualizar_AdministradorNoPuedeDesactivarseNiQuitarseElRol(bool activo, string rol)
        {
            _repo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(new Usuario { Id = 1, Rol = RolesSistema.Administrador });
            var request = new ActualizarUsuarioRequest { Nombre = "Admin", Apellido = "Sistema", Rol = rol, Activo = activo };

            Assert.Equal(StatusCodes.Status400BadRequest,
                await CodigoDeError(() => Servicio().ActualizarAsync(1, request, Usuarios.Administrador(id: 1))));
        }

        [Fact]
        public async Task Actualizar_DesactivaAOtroUsuario()
        {
            _repo.Setup(r => r.ObtenerPorIdAsync(4)).ReturnsAsync(new Usuario { Id = 4, Correo = "carlos@uta.edu.ec", Rol = RolesSistema.Docente });
            var request = new ActualizarUsuarioRequest
            {
                Nombre = "Carlos",
                Apellido = "Gómez",
                Rol = RolesSistema.Docente,
                Cedula = Usuarios.CedulaCarlos,
                Activo = false
            };

            var actualizado = await Servicio().ActualizarAsync(4, request, Usuarios.Administrador(id: 1));

            Assert.False(actualizado.Activo);
            _repo.Verify(r => r.ActualizarAsync(It.Is<Usuario>(u => u.Id == 4 && !u.Activo)));
        }

        [Fact]
        public async Task Actualizar_UsuarioInexistente_Da404()
        {
            var request = new ActualizarUsuarioRequest { Nombre = "X", Apellido = "Y", Rol = RolesSistema.RRHH };

            Assert.Equal(StatusCodes.Status404NotFound,
                await CodigoDeError(() => Servicio().ActualizarAsync(99, request, Usuarios.Administrador())));
        }

        [Fact]
        public async Task CambiarContrasena_GuardaUnHashNuevo()
        {
            _repo.Setup(r => r.ObtenerPorIdAsync(4)).ReturnsAsync(new Usuario { Id = 4 });

            await Servicio().CambiarContrasenaAsync(4, new CambiarContrasenaRequest { Contrasena = "NuevaClave2026" });

            _repo.Verify(r => r.CambiarContrasenaAsync(4, It.Is<string>(h => EsHashDe(h, "NuevaClave2026"))));
        }

        // BCrypt.Verify tiene parámetros opcionales, que no se permiten dentro de It.Is
        private static bool EsHashDe(string hash, string contrasena) => BCrypt.Net.BCrypt.Verify(contrasena, hash);
    }
}
