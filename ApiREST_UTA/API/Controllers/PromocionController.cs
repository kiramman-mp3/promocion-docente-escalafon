using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("validar")]
    public async Task<IActionResult> ValidarPromocion([FromBody] RequisitoRequest request)
    {
        var resultado = await _promocionService.ValidarPromocionCompleta(request);
        return Ok(resultado);
    }
}
