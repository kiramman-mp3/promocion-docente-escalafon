using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Interfaces;

namespace ApiREST_UTA.Application.Services
{
    public class ObraService
    {
        private readonly IObraRepository _obraRepository;

        public ObraService(IObraRepository obraRepository)
        {
            _obraRepository = obraRepository;
        }

        // Modificar este método para aceptar 'FechaInicio' y recuperar las obras después de esa fecha
        public async Task<IEnumerable<ObraResponse>> GetObrasPorCedulaAsync(string cedula, DateTime fechaInicio)
        {
            var obras = await _obraRepository.GetObrasPorCedulaAsync(cedula, fechaInicio);

            return obras.Select(obra => new ObraResponse
            {
                Cedula = obra.Cedula,
                FechaPublicacion = obra.FechaPublicacion,
                NombreRevista = obra.NombreRevista,
                PdfBase64 = Convert.ToBase64String(Convert.FromBase64String(obra.PdfBase64))
            });
        }
    }
}
