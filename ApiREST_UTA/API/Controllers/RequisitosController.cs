using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/requisitos")]
    public class RequisitosController : ControllerBase
    {
        private readonly RequisitoService _service;

        public RequisitosController(RequisitoService service)
        {
            _service = service;
        }

        // Requisitos vigentes (o todo el historial con ?historico=true). Son públicos, como el escalafón.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Listar([FromQuery] bool historico = false)
        {
            return Ok(await _service.ListarAsync(historico));
        }

        // Crea una nueva versión: cierra la vigente del mismo grado el día anterior a VigenteDesde
        [HttpPost]
        [Authorize(Roles = RolesSistema.Administrador)]
        public async Task<IActionResult> Crear([FromBody] RequisitoPromocionRequest request)
        {
            try
            {
                return StatusCode(StatusCodes.Status201Created, await _service.CrearAsync(request, User));
            }
            catch (ReglaNegocioException ex)
            {
                return StatusCode(ex.StatusCode, new { mensaje = ex.Message });
            }
        }
    }
}
