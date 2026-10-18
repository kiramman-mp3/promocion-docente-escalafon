using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Domain.Interfaces.ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiREST_UTA.API.Controllers
{
    [ApiController]
    [Route("api/importar")]
    [SoloCedulaPropia]
    public class ImportacionController : ControllerBase
    {
        private readonly IRRHHRepository _rrhhRepo;
        private readonly IObraRepository _obraRepo;
        private readonly ICursoRepository _cursoRepo;
        private readonly IInvestigacionRepository _investigacionRepo;
        private readonly IEvaluacionDocenteRepository _evaluacionRepo; 
        private readonly ITesisRepository _tesisRepo;
        private readonly IObraIdiomaRepository _obraIdiomaRepo;
        private readonly IProyectoPonderadoRepository _proyectoRepo;
        private readonly ICapacitacionImpartidaRepository _capacitacionRepo;

        public ImportacionController(
                    IRRHHRepository rrhhRepo,
                    IObraRepository obraRepo,
                    ICursoRepository cursoRepo,
                    IInvestigacionRepository investigacionRepo,
                    IEvaluacionDocenteRepository evaluacionRepo,
                    ITesisRepository tesisRepo,
                    IObraIdiomaRepository obraIdiomaRepo,
                    IProyectoPonderadoRepository proyectoRepo,
                    ICapacitacionImpartidaRepository capacitacionRepo)
        {
            _rrhhRepo = rrhhRepo;
            _obraRepo = obraRepo;
            _cursoRepo = cursoRepo;
            _investigacionRepo = investigacionRepo;
            _evaluacionRepo = evaluacionRepo;
            _tesisRepo = tesisRepo;
            _obraIdiomaRepo = obraIdiomaRepo;
            _proyectoRepo = proyectoRepo;
            _capacitacionRepo = capacitacionRepo;
        }
        [Authorize]
        [HttpPost("tiempo-rol")]
        public async Task<IActionResult> ImportarTiempoRol([FromBody] TiempoRolRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var resultado = await _rrhhRepo.ObtenerTiempoRolPorCedulaAsync(request.Cedula);

            if (resultado == null)
                return NotFound(new { mensaje = "No se encontró información" });

            return Ok(resultado);
        }

        [Authorize]
        [HttpPost("obras")]
        public async Task<IActionResult> ImportarObras([FromBody] ObraRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            if (request.FechaInicio == default)
                return BadRequest(new { mensaje = "La fecha de inicio es obligatoria" });

            var obras = await _obraRepo.GetObrasPorCedulaAsync(request.Cedula, request.FechaInicio);

            if (obras == null || !obras.Any())
                return NotFound(new { mensaje = "No se encontraron obras para la cédula después de la fecha proporcionada." });

            return Ok(obras);
        }

        [Authorize]
        [HttpPost("cursos")]
        public async Task<IActionResult> ImportarCursos([FromBody] CursosRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var cursos = await _cursoRepo.ObtenerCursosPorCedula(request.Cedula);

            if (cursos == null || !cursos.Any())
                return NotFound(new { mensaje = "No se encontraron cursos de capacitación para la cédula proporcionada." });

            return Ok(cursos);
        }

        [Authorize]
        [HttpPost("investigaciones")]
        public async Task<IActionResult> ImportarInvestigaciones([FromBody] InvestigacionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var investigaciones = await _investigacionRepo.ObtenerInvestigacionesPorCedula(request.Cedula);

            if (investigaciones == null || !investigaciones.Any())
                return NotFound(new { mensaje = "No se encontraron investigaciones para la cédula proporcionada." });

            return Ok(investigaciones);
        }

        [Authorize]
        [HttpPost("evaluaciones")]
        public async Task<IActionResult> ImportarEvaluaciones([FromBody] EvaluacionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var evaluaciones = await _evaluacionRepo.ObtenerEvaluacionesPorCedula(request.Cedula);

            if (evaluaciones == null || !evaluaciones.Any())
                return NotFound(new { mensaje = "No se encontraron evaluaciones docentes para la cédula proporcionada." });

            return Ok(evaluaciones);
        }

        [Authorize]
        [HttpPost("tesis-doctorado")]
        public async Task<IActionResult> ObtenerTesis([FromBody] TesisDoctoradoRequest req)
        {
            var data = await _tesisRepo.ObtenerTesisPorCedulaAsync(req.Cedula);
            return Ok(new TesisDoctoradoResponse { Cedula = req.Cedula, Cantidad = data });
        }

        [Authorize]
        [HttpPost("obras-idioma")]
        public async Task<IActionResult> VerificarObraIdioma([FromBody] ObraIdiomaRequest req)
        {
            var tiene = await _obraIdiomaRepo.TieneObraEnIdiomaExtranjero(req.Cedula);
            return Ok(new ObraIdiomaResponse { Cedula = req.Cedula, TieneObraEnOtroIdioma = tiene });
        }

        [Authorize]
        [HttpPost("proyectos-ponderados")]
        public async Task<IActionResult> ObtenerMesesPonderados([FromBody] ProyectoPonderadoRequest req)
        {
            var meses = await _proyectoRepo.ObtenerMesesPonderadosAsync(req.Cedula);
            return Ok(new ProyectoPonderadoResponse { Cedula = req.Cedula, MesesPonderados = meses });
        }

        [Authorize]
        [HttpPost("capacitacion-impartida")]
        public async Task<IActionResult> ObtenerHorasImpartidas([FromBody] CapacitacionImpartidaRequest req)
        {
            var horas = await _capacitacionRepo.ObtenerHorasImpartidasAsync(req.Cedula);
            return Ok(new CapacitacionImpartidaResponse { Cedula = req.Cedula, HorasImpartidas = horas });
        }


    }
}