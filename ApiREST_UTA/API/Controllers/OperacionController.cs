using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    // Bitácora de auditoría e informe de funcionamiento y utilización (Reglamento DITIC arts. 2 y 18 c)
    [ApiController]
    [Route("api")]
    [Authorize(Roles = RolesSistema.Administrador)]
    public class OperacionController : ControllerBase
    {
        private readonly IAuditoriaRepository _auditoria;

        public OperacionController(IAuditoriaRepository auditoria)
        {
            _auditoria = auditoria;
        }

        [HttpGet("auditoria")]
        public async Task<IActionResult> Auditoria([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] int? usuarioId, [FromQuery] string? accion, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50)
        {
            var filtro = new FiltroAuditoria
            {
                Desde = desde,
                Hasta = hasta,
                UsuarioId = usuarioId,
                Accion = string.IsNullOrWhiteSpace(accion) ? null : accion.Trim(),
                Pagina = Math.Max(pagina, 1),
                TamanoPagina = Math.Clamp(tamanoPagina, 1, 200)
            };
            var (registros, total) = await _auditoria.ConsultarAsync(filtro);
            return Ok(new { total, pagina = filtro.Pagina, tamanoPagina = filtro.TamanoPagina, registros });
        }

        [HttpGet("reportes/uso")]
        public async Task<IActionResult> ReporteUso()
        {
            return Ok(await _auditoria.ReporteUsoAsync());
        }
    }
}
