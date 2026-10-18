using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiREST_UTA.API.Filters
{
    // Registra en la bitácora cada operación de escritura y cada descarga de documentos (Reglamento DITIC art. 2).
    // Nunca guarda contraseñas, tokens ni el contenido de los archivos.
    public class AuditoriaFilter : IAsyncActionFilter
    {
        private static readonly string[] CamposSensibles = { "contrasena", "password", "token" };
        private static readonly JsonSerializerOptions JsonWeb = new(JsonSerializerDefaults.Web);

        private readonly IAuditoriaRepository _auditoria;
        private readonly ILogger<AuditoriaFilter> _logger;

        public AuditoriaFilter(IAuditoriaRepository auditoria, ILogger<AuditoriaFilter> logger)
        {
            _auditoria = auditoria;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var metodo = context.HttpContext.Request.Method;
            var esDescarga = descriptor?.ActionName.StartsWith("Descargar") == true;
            if (HttpMethods.IsGet(metodo) && !esDescarga)
            {
                await next();
                return;
            }

            var detalle = Detalle(context.ActionArguments);
            var ejecutado = await next();

            var estado = ejecutado.Exception != null && !ejecutado.ExceptionHandled
                ? StatusCodes.Status500InternalServerError
                : ejecutado.Result is IStatusCodeActionResult conEstado ? conEstado.StatusCode ?? 200 : 200;

            var usuario = context.HttpContext.User;
            var idTexto = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
            var correo = usuario.FindFirstValue(ClaimTypes.Name)
                ?? context.ActionArguments.Values.Select(a => a?.GetType().GetProperty("Correo")?.GetValue(a) as string).FirstOrDefault(c => c != null);

            try
            {
                await _auditoria.RegistrarAsync(new RegistroAuditoria
                {
                    UsuarioId = int.TryParse(idTexto, out var id) ? id : null,
                    Usuario = correo,
                    Accion = $"{descriptor?.ControllerName}.{descriptor?.ActionName}",
                    Metodo = metodo,
                    Ruta = context.HttpContext.Request.Path,
                    Estado = estado,
                    Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Detalle = detalle
                });
            }
            catch (Exception ex)
            {
                // La bitácora no debe tumbar la operación del usuario; queda en el log del servidor
                _logger.LogError(ex, "No se pudo registrar la auditoría de {Ruta}", context.HttpContext.Request.Path);
            }
        }

        private static string? Detalle(IDictionary<string, object?> argumentos)
        {
            if (argumentos.Count == 0)
                return null;

            var detalle = new JsonObject();
            foreach (var (nombre, valor) in argumentos)
            {
                detalle[nombre] = valor switch
                {
                    null => null,
                    IFormFile archivo => new JsonObject { ["archivo"] = archivo.FileName, ["bytes"] = archivo.Length },
                    _ => Limpiar(JsonSerializer.SerializeToNode(valor, JsonWeb))
                };
            }
            return detalle.ToJsonString();
        }

        private static JsonNode? Limpiar(JsonNode? nodo)
        {
            if (nodo is JsonObject objeto)
            {
                foreach (var clave in objeto.Select(p => p.Key).ToList())
                {
                    if (CamposSensibles.Any(s => clave.Contains(s, StringComparison.OrdinalIgnoreCase)))
                        objeto[clave] = "***";
                    else
                        Limpiar(objeto[clave]);
                }
            }
            return nodo;
        }
    }
}
