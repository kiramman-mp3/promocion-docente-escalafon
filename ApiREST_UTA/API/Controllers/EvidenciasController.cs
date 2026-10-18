using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/solicitudes/{solicitudId:int}/evidencias")]
    [Authorize]
    public class EvidenciasController : ControllerBase
    {
        private readonly EvidenciaService _service;

        public EvidenciasController(EvidenciaService service)
        {
            _service = service;
        }

        [HttpGet]
        public Task<IActionResult> Listar(int solicitudId) =>
            Ejecutar(async () => Ok(await _service.ListarAsync(solicitudId, User)));

        // multipart/form-data: archivo (PDF de hasta 10 MB), tipo y descripcion
        [HttpPost]
        [RequestSizeLimit(EvidenciaSolicitud.TamanoMaximo + 1024 * 1024)]
        public Task<IActionResult> Subir(int solicitudId, [FromForm] IFormFile? archivo, [FromForm] string? tipo, [FromForm] string? descripcion) =>
            Ejecutar(async () =>
            {
                if (archivo == null)
                    return BadRequest(new { mensaje = "Adjunta el archivo PDF." });

                using var memoria = new MemoryStream();
                await archivo.CopyToAsync(memoria);
                var evidencia = await _service.SubirAsync(solicitudId, tipo ?? "", descripcion ?? "", archivo.FileName, memoria.ToArray(), User);
                return StatusCode(StatusCodes.Status201Created, evidencia);
            });

        [HttpGet("{evidenciaId:int}")]
        public Task<IActionResult> Descargar(int solicitudId, int evidenciaId) =>
            Ejecutar(async () =>
            {
                var evidencia = await _service.DescargarAsync(solicitudId, evidenciaId, User);
                return File(evidencia.Contenido!, "application/pdf", evidencia.NombreArchivo);
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
