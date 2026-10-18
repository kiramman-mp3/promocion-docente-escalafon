using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/comision/miembros")]
    [Authorize]
    public class ComisionController : ControllerBase
    {
        private readonly ComisionService _service;

        public ComisionController(ComisionService service)
        {
            _service = service;
        }

        // Conformación vigente (o histórica con ?historico=true): visible para todo usuario autenticado
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool historico = false)
        {
            return Ok(await _service.ListarAsync(historico));
        }

        [HttpPost]
        [Authorize(Roles = RolesSistema.Administrador)]
        public Task<IActionResult> Agregar([FromBody] MiembroComisionRequest request) =>
            Ejecutar(async () => StatusCode(StatusCodes.Status201Created, await _service.AgregarAsync(request)));

        [HttpPost("{id:int}/finalizar")]
        [Authorize(Roles = RolesSistema.Administrador)]
        public Task<IActionResult> Finalizar(int id) =>
            Ejecutar(async () =>
            {
                await _service.FinalizarAsync(id);
                return NoContent();
            });

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
