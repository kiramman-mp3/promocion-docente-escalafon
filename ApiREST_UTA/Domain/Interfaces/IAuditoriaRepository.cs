using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task RegistrarAsync(RegistroAuditoria registro);
        Task<(List<RegistroAuditoria> Registros, int Total)> ConsultarAsync(FiltroAuditoria filtro);
        Task<ReporteUso> ReporteUsoAsync();
    }
}
