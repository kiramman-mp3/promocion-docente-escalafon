using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IObraRepository
    {
        // Obras con PDF, publicadas desde fechaInicio (DateTime.MinValue = todas)
        Task<IEnumerable<ObraResponse>> GetObrasPorCedulaAsync(string cedula, DateTime fechaInicio);

        // Obras sin PDF para evaluar requisitos
        Task<List<Obra>> ObtenerObrasAsync(string cedula);
    }
}
