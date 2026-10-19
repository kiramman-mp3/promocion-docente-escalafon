using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ApiREST_UTA.Tests
{
    // Reglamento DITIC art. 2: bitácora de las operaciones, sin exponer secretos
    public class AuditoriaFilterTests
    {
        private static ActionExecutingContext Contexto(string metodo, string accion, object argumento)
        {
            var http = new DefaultHttpContext();
            http.Request.Method = metodo;
            http.Request.Path = "/api/prueba";
            var descriptor = new ControllerActionDescriptor { ControllerName = "Auth", ActionName = accion };
            return new ActionExecutingContext(
                new ActionContext(http, new RouteData(), descriptor),
                new List<IFilterMetadata>(),
                new Dictionary<string, object?> { ["request"] = argumento },
                controller: new object());
        }

        private static ActionExecutionDelegate Responder(ActionExecutingContext c, IActionResult resultado) =>
            () => Task.FromResult(new ActionExecutedContext(c, new List<IFilterMetadata>(), c.Controller) { Result = resultado });

        [Fact]
        public async Task NoGuardaContrasenas_YRegistraElIntentoDeLogin()
        {
            var repo = new Mock<IAuditoriaRepository>();
            RegistroAuditoria? registro = null;
            repo.Setup(r => r.RegistrarAsync(It.IsAny<RegistroAuditoria>())).Callback<RegistroAuditoria>(r => registro = r).Returns(Task.CompletedTask);
            var filtro = new AuditoriaFilter(repo.Object, NullLogger<AuditoriaFilter>.Instance);
            var contexto = Contexto("POST", "Login", new LoginRequest { Correo = "ana.perez@uta.edu.ec", Contrasena = "Secreta2026!" });

            await filtro.OnActionExecutionAsync(contexto, Responder(contexto, new UnauthorizedResult()));

            Assert.NotNull(registro);
            Assert.Equal("Auth.Login", registro!.Accion);
            Assert.Equal(401, registro.Estado);
            Assert.Equal("ana.perez@uta.edu.ec", registro.Usuario);
            Assert.DoesNotContain("Secreta2026!", registro.Detalle);
            Assert.Contains("***", registro.Detalle);
        }

        [Fact]
        public async Task LasConsultasNoSeRegistran()
        {
            var repo = new Mock<IAuditoriaRepository>();
            var filtro = new AuditoriaFilter(repo.Object, NullLogger<AuditoriaFilter>.Instance);
            var contexto = Contexto("GET", "Listar", new object());

            await filtro.OnActionExecutionAsync(contexto, Responder(contexto, new OkResult()));

            repo.Verify(r => r.RegistrarAsync(It.IsAny<RegistroAuditoria>()), Times.Never);
        }

        [Fact]
        public async Task SiLaBitacoraFalla_LaOperacionContinua()
        {
            var repo = new Mock<IAuditoriaRepository>();
            repo.Setup(r => r.RegistrarAsync(It.IsAny<RegistroAuditoria>())).ThrowsAsync(new InvalidOperationException("sin conexión"));
            var filtro = new AuditoriaFilter(repo.Object, NullLogger<AuditoriaFilter>.Instance);
            var contexto = Contexto("POST", "Crear", new CrearSolicitudRequest { Cedula = Usuarios.CedulaAna });

            await filtro.OnActionExecutionAsync(contexto, Responder(contexto, new OkResult()));
        }
    }
}
