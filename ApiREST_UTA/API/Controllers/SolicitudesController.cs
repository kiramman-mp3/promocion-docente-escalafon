using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/solicitudes")]
    [Authorize]
    public class SolicitudesController : ControllerBase
    {
        private readonly SolicitudService _service;

        public SolicitudesController(SolicitudService service)
        {
            _service = service;
        }

        // Docente: solicita su propia promoción. RRHH/Administrador: pueden registrarla para cualquier docente.
        [HttpPost]
        [SoloCedulaPropia]
        public Task<IActionResult> Crear([FromBody] CrearSolicitudRequest request) =>
            Ejecutar(async () =>
            {
                var solicitud = await _service.CrearAsync(request.Cedula, User);
                return CreatedAtAction(nameof(Obtener), new { id = solicitud.Id }, solicitud);
            });

        // Docente: sus solicitudes. RRHH/Administrador: todas (filtro opcional ?estado=Pendiente)
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? estado)
        {
            return Ok(await _service.ListarAsync(estado, User));
        }

        [HttpGet("{id:int}")]
        public Task<IActionResult> Obtener(int id) =>
            Ejecutar(async () =>
            {
                var solicitud = await _service.ObtenerAsync(id, User);
                return solicitud == null ? NotFound(new { mensaje = "La solicitud no existe." }) : Ok(solicitud);
            });

        [HttpPut("{id:int}/documentos")]
        [Authorize(Roles = RolesSistema.Revisores)]
        public Task<IActionResult> RevisarDocumento(int id, [FromBody] RevisarDocumentoRequest request) =>
            Ejecutar(async () => Ok(await _service.RevisarDocumentoAsync(id, request, User)));

        [HttpPut("{id:int}/resolucion")]
        [Authorize(Roles = RolesSistema.Revisores)]
        public Task<IActionResult> Resolver(int id, [FromBody] ResolverSolicitudRequest request) =>
            Ejecutar(async () => Ok(await _service.ResolverAsync(id, request, User)));

        private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
        {
            try
            {
                return await accion();
            }
            catch (ReglaNegocioException ex)
            {
                return StatusCode(ex.StatusCode, new { mensaje = ex.Message });
            }
        }
    }
}
