namespace ApiREST_UTA.Domain.Interfaces
{
    public interface IObraIdiomaRepository
    {
        Task<bool> TieneObraEnIdiomaExtranjero(string cedula);
    }
}
