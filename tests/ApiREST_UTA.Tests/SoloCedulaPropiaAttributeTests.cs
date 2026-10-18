using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace ApiREST_UTA.Tests
{
    public class SoloCedulaPropiaAttributeTests
    {
        private static ActionExecutingContext Contexto(ClaimsPrincipal usuario, object cuerpo)
        {
            var httpContext = new DefaultHttpContext { User = usuario };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?> { ["request"] = cuerpo },
                controller: new object());
        }

        [Fact]
        public void Docente_ConCedulaAjena_Recibe403()
        {
            var contexto = Contexto(Usuarios.Docente(), new TiempoRolRequest { Cedula = Usuarios.CedulaCarlos });

            new SoloCedulaPropiaAttribute().OnActionExecuting(contexto);

            var resultado = Assert.IsType<ObjectResult>(contexto.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, resultado.StatusCode);
        }

        [Fact]
        public void Docente_ConSuCedula_Continua()
        {
            var contexto = Contexto(Usuarios.Docente(), new RequisitoRequest { Cedula = Usuarios.CedulaAna, RolActualId = 3 });

            new SoloCedulaPropiaAttribute().OnActionExecuting(contexto);

            Assert.Null(contexto.Result);
        }

        [Fact]
        public void Rrhh_ConCualquierCedula_Continua()
        {
            var contexto = Contexto(Usuarios.Rrhh(), new CrearSolicitudRequest { Cedula = Usuarios.CedulaCarlos });

            new SoloCedulaPropiaAttribute().OnActionExecuting(contexto);

            Assert.Null(contexto.Result);
        }
    }
}
