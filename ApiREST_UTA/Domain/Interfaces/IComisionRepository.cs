using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IComisionRepository
    {
        Task<List<MiembroComision>> ListarMiembrosAsync(bool incluirHistorico);
        Task<MiembroComision?> ObtenerMiembroActivoPorUsuarioAsync(int usuarioId);
        Task<MiembroComision?> ObtenerMiembroAsync(int id);
        Task<int> AgregarMiembroAsync(MiembroComision miembro);
        Task FinalizarMiembroAsync(int id, DateTime hasta);

        Task<List<VotoComision>> ObtenerVotosAsync(int solicitudId);

        // Inserta o reemplaza el voto del miembro en la solicitud
        Task GuardarVotoAsync(VotoComision voto);
    }
}
