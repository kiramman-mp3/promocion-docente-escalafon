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

        // Docente: solicita su propia promoción (CES art. 99). Talento Humano/Administrador: pueden registrarla para cualquier docente.
        [HttpPost]
        [SoloCedulaPropia]
        public Task<IActionResult> Crear([FromBody] CrearSolicitudRequest request) =>
            Ejecutar(async () =>
            {
                var solicitud = await _service.CrearAsync(request.Cedula, User);
                return CreatedAtAction(nameof(Obtener), new { id = solicitud.Id }, solicitud);
            });

        // Docente: sus solicitudes. Comisión: además las enviadas a ella. Talento Humano/Administrador: todas.
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

        // Revisión documental por Talento Humano
        [HttpPut("{id:int}/documentos")]
        [Authorize(Roles = RolesSistema.Revisores)]
        public Task<IActionResult> RevisarDocumento(int id, [FromBody] RevisarDocumentoRequest request) =>
            Ejecutar(async () => Ok(await _service.RevisarDocumentoAsync(id, request, User)));

        [HttpPost("{id:int}/enviar-comision")]
        [Authorize(Roles = RolesSistema.Revisores)]
        public Task<IActionResult> EnviarAComision(int id) =>
            Ejecutar(async () => Ok(await _service.EnviarAComisionAsync(id, User)));

        // Votación de la Comisión Académica (UTA art. 47). La pertenencia se valida en el servicio.
        [HttpPut("{id:int}/votos")]
        public Task<IActionResult> Votar(int id, [FromBody] VotoRequest request) =>
            Ejecutar(async () => Ok(await _service.VotarAsync(id, request, User)));

        [HttpPost("{id:int}/cerrar-votacion")]
        public Task<IActionResult> CerrarVotacion(int id, [FromBody] CerrarVotacionRequest request) =>
            Ejecutar(async () => Ok(await _service.CerrarVotacionAsync(id, request, User)));

        // Resolución de la Comisión en PDF (acto administrativo, CES art. 77 l)
        [HttpGet("{id:int}/acto")]
        public Task<IActionResult> DescargarActo(int id) =>
            Ejecutar(async () =>
            {
                var (pdf, nombre) = await _service.DescargarActoAsync(id, User);
                return File(pdf, "application/pdf", nombre);
            });

        // Talento Humano registra la acción de personal: la solicitud pasa a Ejecutada
        [HttpPut("{id:int}/accion-personal")]
        [Authorize(Roles = RolesSistema.Revisores)]
        public Task<IActionResult> RegistrarAccionPersonal(int id, [FromBody] AccionPersonalRequest request) =>
            Ejecutar(async () => Ok(await _service.RegistrarAccionPersonalAsync(id, request, User)));

        // Apelación del docente (COA arts. 217 y 224) y registro de lo que resolvió el Consejo Universitario
        [HttpPost("{id:int}/apelacion")]
        public Task<IActionResult> Apelar(int id, [FromBody] ApelarRequest request) =>
            Ejecutar(async () => Ok(await _service.ApelarAsync(id, request, User)));

        [HttpPut("{id:int}/apelacion/resolucion")]
        [Authorize(Roles = RolesSistema.Administrador)]
        public Task<IActionResult> ResolverApelacion(int id, [FromBody] ResolverApelacionRequest request) =>
            Ejecutar(async () => Ok(await _service.ResolverApelacionAsync(id, request, User)));

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
