using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(Roles = RolesSistema.Administrador)]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuariosController(UsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await _service.ListarAsync());
        }

        [HttpPost]
        public Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request) =>
            Ejecutar(async () => StatusCode(StatusCodes.Status201Created, await _service.CrearAsync(request)));

        [HttpPut("{id:int}")]
        public Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioRequest request) =>
            Ejecutar(async () => Ok(await _service.ActualizarAsync(id, request, User)));

        [HttpPut("{id:int}/contrasena")]
        public Task<IActionResult> CambiarContrasena(int id, [FromBody] CambiarContrasenaRequest request) =>
            Ejecutar(async () =>
            {
                await _service.CambiarContrasenaAsync(id, request);
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
