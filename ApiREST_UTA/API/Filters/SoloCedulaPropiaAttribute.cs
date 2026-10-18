using ApiREST_UTA.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiREST_UTA.API.Filters
{
    // Rechaza con 403 cuando un docente pide datos de una cédula que no es la suya.
    // Busca la propiedad "Cedula" en el cuerpo de la petición (todos los *Request la tienen).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SoloCedulaPropiaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cedula = context.ActionArguments.Values
                .Select(arg => arg?.GetType().GetProperty("Cedula")?.GetValue(arg) as string)
                .FirstOrDefault(c => c != null);

            if (!context.HttpContext.User.PuedeConsultar(cedula))
            {
                context.Result = new ObjectResult(new { mensaje = "Solo puedes consultar tu propio expediente." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
