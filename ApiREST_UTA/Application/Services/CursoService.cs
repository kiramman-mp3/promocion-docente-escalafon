using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Interfaces;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace ApiREST_UTA.Application.Services
{
    public class CursoService
    {
        private readonly ICursoRepository _repo;

        public CursoService(ICursoRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CursosResponse>> ObtenerCursos(string cedula)
        {
            var cursos = await _repo.ObtenerCursosPorCedula(cedula);
            return cursos.Select(c => new CursosResponse
            {
                Cedula = c.Cedula,
                Universidad = c.Universidad,
                Facultad = c.Facultad,
                FechaEvaluacion = c.FechaEvaluacion,
                Puntaje = c.Puntaje
            }).ToList();
        }
    }
}
