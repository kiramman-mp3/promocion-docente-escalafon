using ApiREST_UTA.Application.Services.Promocion;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;

namespace ApiREST_UTA.Application.Services
{
    // Arma el contexto de un docente desde las cinco bases y obtiene el dictamen de promoción
    public class PromocionService
    {
        private readonly IRolRepository _rolRepo;
        private readonly IRRHHRepository _rrhhRepo;
        private readonly IObraRepository _obraRepo;
        private readonly IEvaluacionDocenteRepository _evalRepo;
        private readonly ICursoRepository _cursoRepo;
        private readonly IEquivalenciaRepository _equivalenciaRepo;
        private readonly IProyectoPonderadoRepository _proyectoRepo;
        private readonly ITesisRepository _tesisRepo;
        private readonly IRequisitoRepository _requisitoRepo;
        private readonly MotorPromocion _motor = new();

        public PromocionService(
            IRolRepository rolRepo,
            IRRHHRepository rrhhRepo,
            IObraRepository obraRepo,
            IEvaluacionDocenteRepository evalRepo,
            ICursoRepository cursoRepo,
            IEquivalenciaRepository equivalenciaRepo,
            IProyectoPonderadoRepository proyectoRepo,
            ITesisRepository tesisRepo,
            IRequisitoRepository requisitoRepo)
        {
            _rolRepo = rolRepo;
            _rrhhRepo = rrhhRepo;
            _obraRepo = obraRepo;
            _evalRepo = evalRepo;
            _cursoRepo = cursoRepo;
            _equivalenciaRepo = equivalenciaRepo;
            _proyectoRepo = proyectoRepo;
            _tesisRepo = tesisRepo;
            _requisitoRepo = requisitoRepo;
        }

        // Dictamen con los requisitos vigentes hoy, o con los de una solicitud ya presentada
        // (CES, transitoria novena: rigen las normas vigentes al presentar la solicitud)
        public async Task<DictamenPromocion> EvaluarAsync(string cedula, int? requisitosId = null, DateTime? fechaCorte = null)
        {
            var contexto = await ConstruirContextoAsync(cedula.Trim(), requisitosId, fechaCorte ?? DateTime.Today);
            return _motor.Evaluar(contexto);
        }

        public async Task<ContextoPromocion> ConstruirContextoAsync(string cedula, int? requisitosId, DateTime fechaCorte)
        {
            var tiempo = await _rrhhRepo.ObtenerTiempoRolPorCedulaAsync(cedula)
                ?? throw ReglaNegocioException.NoEncontrado($"RRHH no tiene registrada la cédula {cedula}.");

            var roles = await _rolRepo.ListarAsync();
            var rolActual = roles.FirstOrDefault(r => string.Equals(r.Nombre, tiempo.RolActual.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw ReglaNegocioException.Invalido($"El rol \"{tiempo.RolActual}\" registrado en RRHH no está en el escalafón.");

            var requisitos = requisitosId.HasValue
                ? await _requisitoRepo.ObtenerPorIdAsync(requisitosId.Value)
                : rolActual.Promocionable ? await _requisitoRepo.ObtenerVigenteAsync(rolActual.Id, fechaCorte) : null;

            return new ContextoPromocion
            {
                Cedula = cedula,
                FechaCorte = fechaCorte.Date,
                Roles = roles,
                RolActual = rolActual,
                RolObjetivo = rolActual.Promocionable ? roles.FirstOrDefault(r => r.Id == rolActual.Id + 1) : null,
                TiempoRolId = tiempo.Id,
                FechaInicioGrado = tiempo.FechaInicio.Date,
                Requisitos = requisitos,
                Trayectoria = await _rrhhRepo.ObtenerTrayectoriaAsync(cedula),
                CargosAutoridad = await _rrhhRepo.ObtenerCargosAutoridadAsync(cedula),
                Sabaticos = await _rrhhRepo.ObtenerSabaticosAsync(cedula),
                Obras = await _obraRepo.ObtenerObrasAsync(cedula),
                Evaluaciones = await _evalRepo.ObtenerEvaluacionesPorCedula(cedula),
                Cursos = (await _cursoRepo.ObtenerCursosPorCedula(cedula)).ToList(),
                Equivalencias = await _equivalenciaRepo.ObtenerEquivalenciasAsync(cedula),
                Proyectos = await _proyectoRepo.ObtenerProyectosAsync(cedula),
                TrabajosTitulacion = await _tesisRepo.ObtenerTrabajosTitulacionAsync(cedula)
            };
        }
    }
}
