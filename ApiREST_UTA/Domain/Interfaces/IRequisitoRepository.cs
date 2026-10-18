using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IRequisitoRepository
    {
        Task<List<RequisitoPromocion>> ListarAsync(bool incluirHistorico);
        Task<RequisitoPromocion?> ObtenerPorIdAsync(int id);
        Task<RequisitoPromocion?> ObtenerVigenteAsync(int gradoOrigen, DateTime fecha);
        Task<RequisitoPromocion?> ObtenerAbiertoAsync(int gradoOrigen);

        // Cierra el requisito abierto del mismo grado (VigenteHasta = día anterior) e inserta el nuevo, en una transacción
        Task<int> ReemplazarAsync(RequisitoPromocion nuevo);
    }
}
