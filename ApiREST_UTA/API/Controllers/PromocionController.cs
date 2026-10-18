using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [Route("api/promocion")]
    [ApiController]
    [Authorize]
    [SoloCedulaPropia]
    public class PromocionController : ControllerBase
    {
        private readonly PromocionService _promocionService;

        public PromocionController(PromocionService promocionService)
        {
            _promocionService = promocionService;
        }

        // Dictamen requisito por requisito con los requisitos vigentes. El rol actual se toma de RRHH.
        [HttpPost("validar")]
        public async Task<IActionResult> ValidarPromocion([FromBody] RequisitoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria." });

            try
            {
                return Ok(await _promocionService.EvaluarAsync(request.Cedula));
            }
            catch (ReglaNegocioException ex)
            {
                return StatusCode(ex.StatusCode, new { mensaje = ex.Message });
            }
        }
    }
}
