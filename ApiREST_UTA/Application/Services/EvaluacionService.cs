using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Interfaces;

namespace ApiREST_UTA.Application.Services
{
    public class EvaluacionDocenteService
    {
        private readonly IEvaluacionDocenteRepository _repo;

        public EvaluacionDocenteService(IEvaluacionDocenteRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<EvaluacionResponse>> ObtenerEvaluaciones(string cedula)
        {
            var evaluaciones = await _repo.ObtenerEvaluacionesPorCedula(cedula);
            return evaluaciones.Select(e => new EvaluacionResponse
            {
                Cedula = e.Cedula,
                Periodo = e.Periodo,
                Universidad = e.Universidad,
                Carrera = e.Carrera,
                Puntaje = e.Puntaje,
                PdfBase64 = Convert.ToBase64String(e.Pdf)
            }).ToList();
        }

    }
}
