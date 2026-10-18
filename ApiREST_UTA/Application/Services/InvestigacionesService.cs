using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Interfaces;

namespace ApiREST_UTA.Application.Services
{
    public class InvestigacionService
    {
        private readonly IInvestigacionRepository _repo;

        public InvestigacionService(IInvestigacionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<InvestigacionResponse>> ObtenerInvestigaciones(string cedula)
        {
            var investigaciones = await _repo.ObtenerInvestigacionesPorCedula(cedula);
            return investigaciones.Select(i => new InvestigacionResponse
            {
                Cedula = i.Cedula,
                Nombre = i.Nombre,
                Encargado = i.Encargado,
                Horas = i.Horas,
                PdfBase64 = Convert.ToBase64String(i.Pdf)
            }).ToList();
        }
    }
}
