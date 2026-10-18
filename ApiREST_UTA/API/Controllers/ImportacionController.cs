using ApiREST_UTA.API.Filters;
using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Application.Services;
using ApiREST_UTA.Domain.Interfaces;
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
        private readonly IEquivalenciaRepository _equivalenciaRepo;

        public ImportacionController(
                    IRRHHRepository rrhhRepo,
                    IObraRepository obraRepo,
                    ICursoRepository cursoRepo,
                    IInvestigacionRepository investigacionRepo,
                    IEvaluacionDocenteRepository evaluacionRepo,
                    ITesisRepository tesisRepo,
                    IObraIdiomaRepository obraIdiomaRepo,
                    IProyectoPonderadoRepository proyectoRepo,
                    ICapacitacionImpartidaRepository capacitacionRepo,
                    IEquivalenciaRepository equivalenciaRepo)
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
            _equivalenciaRepo = equivalenciaRepo;
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

            // Sin FechaInicio devuelve todas las obras (el art. 76 b cuenta toda la producción)
            var obras = await _obraRepo.GetObrasPorCedulaAsync(request.Cedula, request.FechaInicio);

            return Ok(obras);
        }

        [Authorize]
        [HttpPost("cursos")]
        public async Task<IActionResult> ImportarCursos([FromBody] CursosRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var cursos = await _cursoRepo.ObtenerCursosPorCedula(request.Cedula);

            return Ok(cursos);
        }

        [Authorize]
        [HttpPost("investigaciones")]
        public async Task<IActionResult> ImportarInvestigaciones([FromBody] InvestigacionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var investigaciones = await _investigacionRepo.ObtenerInvestigacionesPorCedula(request.Cedula);

            return Ok(investigaciones);
        }

        [Authorize]
        [HttpPost("evaluaciones")]
        public async Task<IActionResult> ImportarEvaluaciones([FromBody] EvaluacionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var evaluaciones = await _evaluacionRepo.ObtenerEvaluacionesPorCedula(request.Cedula);

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

        [Authorize]
        [HttpPost("trayectoria")]
        public async Task<IActionResult> ImportarTrayectoria([FromBody] TiempoRolRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            return Ok(new
            {
                trayectoria = await _rrhhRepo.ObtenerTrayectoriaAsync(request.Cedula),
                cargosAutoridad = await _rrhhRepo.ObtenerCargosAutoridadAsync(request.Cedula),
                sabaticos = await _rrhhRepo.ObtenerSabaticosAsync(request.Cedula)
            });
        }

        [Authorize]
        [HttpPost("proyectos")]
        public async Task<IActionResult> ImportarProyectos([FromBody] ProyectoPonderadoRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var proyectos = await _proyectoRepo.ObtenerProyectosAsync(req.Cedula);
            return Ok(proyectos.Select(p => new
            {
                p.Id,
                p.Cedula,
                p.NombreProyecto,
                p.Rol,
                p.TipoProyecto,
                p.Meses,
                p.MesesPonderados,
                p.FechaInicio,
                p.FechaFin
            }));
        }

        [Authorize]
        [HttpPost("trabajos-titulacion")]
        public async Task<IActionResult> ImportarTrabajosTitulacion([FromBody] TesisDoctoradoRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            return Ok(await _tesisRepo.ObtenerTrabajosTitulacionAsync(req.Cedula));
        }

        [Authorize]
        [HttpPost("equivalencias")]
        public async Task<IActionResult> ImportarEquivalencias([FromBody] CapacitacionImpartidaRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Cedula))
                return BadRequest(new { mensaje = "La cédula es obligatoria" });

            var equivalencias = await _equivalenciaRepo.ObtenerEquivalenciasAsync(req.Cedula);
            return Ok(equivalencias.Select(e => new { e.Id, e.Cedula, e.Tipo, e.Fecha, e.Meses, e.Descripcion, e.HorasCapacitacion }));
        }
    }
}
