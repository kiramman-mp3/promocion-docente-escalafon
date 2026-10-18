using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using ApiREST_UTA.Domain.Interfaces.ApiREST_UTA.Domain.Interfaces;

public class PromocionService
{
    private readonly IRolRepository _rolRepo;
    private readonly IRRHHRepository _rrhhRepo;
    private readonly IObraRepository _obraRepo;
    private readonly IEvaluacionDocenteRepository _evalRepo;
    private readonly ICursoRepository _cursoRepo;
    private readonly IInvestigacionRepository _investigacionRepo;

    public PromocionService(
        IRolRepository rolRepo,
        IRRHHRepository rrhhRepo,
        IObraRepository obraRepo,
        IEvaluacionDocenteRepository evalRepo,
        ICursoRepository cursoRepo,
        IInvestigacionRepository investigacionRepo)
    {
        _rolRepo = rolRepo;
        _rrhhRepo = rrhhRepo;
        _obraRepo = obraRepo;
        _evalRepo = evalRepo;
        _cursoRepo = cursoRepo;
        _investigacionRepo = investigacionRepo;
    }

    public async Task<RequisitoResponse> ValidarPromocionCompleta(RequisitoRequest request)
    {
        var rol = await _rolRepo.ObtenerSiguienteRol(request.RolActualId);
        var tiempo = await _rrhhRepo.ObtenerTiempoRolPorCedulaAsync(request.Cedula);
        var obras = await _obraRepo.GetObrasPorCedulaAsync(request.Cedula, tiempo.FechaInicio);
        var evaluaciones = await _evalRepo.ObtenerEvaluacionesPorCedula(request.Cedula);
        var cursos = await _cursoRepo.ObtenerCursosPorCedula(request.Cedula);
        var investigaciones = await _investigacionRepo.ObtenerInvestigacionesPorCedula(request.Cedula);

        var response = new RequisitoResponse
        {
            RolObjetivo = rol.Nombre,
            CumpleExperiencia = tiempo.AniosEnRol >= rol.RequiereAniosEnRol,
            CumpleObras = obras.Count() >= rol.RequiereObras,
            CumpleEvaluacion = evaluaciones.Count >= 4 &&
                               evaluaciones.Average(e => e.Puntaje) >= rol.RequiereEvaluacion,
            CumpleCapacitacion = ValidarCapacitacion(cursos, rol.RequiereCapacitacionHoras),
            CumpleInvestigacion = ValidarInvestigacion(investigaciones, rol.RequiereInvestigacionMeses),
            CumpleTesisDoctorado = ValidarTesisDoctorado(request.Cedula, rol.Id) // Personalizable
        };

        response.Observaciones = GenerarObservaciones(response);
        return response;
    }

    private bool ValidarCapacitacion(IEnumerable<CursoCapacitacion> cursos, int totalHoras)
    {
        var total = cursos.Sum(c => c.Puntaje);
        return total >= totalHoras;
    }

    private bool ValidarInvestigacion(IEnumerable<Investigacion> investigaciones, int mesesRequeridos)
    {
        // Este ejemplo simplifica: 20h = 1 mes
        var horasTotales = investigaciones.Sum(i => i.Horas);
        return horasTotales >= mesesRequeridos * 20;
    }

    private bool ValidarTesisDoctorado(string cedula, int rolDestinoId)
    {
        if (rolDestinoId < 6) return true; // Solo para Principal 2 y 3
        // Aquí debes consultar DIDE/DAC para contar tesis dirigidas
        return false;
    }

    private string GenerarObservaciones(RequisitoResponse r)
    {
        var obs = new List<string>();
        if (!r.CumpleExperiencia) obs.Add("No cumple tiempo mínimo en el rol actual.");
        if (!r.CumpleObras) obs.Add("No cumple número mínimo de obras con filiación UTA.");
        if (!r.CumpleEvaluacion) obs.Add("Promedio de evaluación docente insuficiente.");
        if (!r.CumpleCapacitacion) obs.Add("Horas de capacitación incompletas.");
        if (!r.CumpleInvestigacion) obs.Add("No cumple con los meses requeridos en proyectos.");
        if (!r.CumpleTesisDoctorado) obs.Add("No ha dirigido tesis de doctorado.");
        return string.Join(" ", obs);
    }
}
