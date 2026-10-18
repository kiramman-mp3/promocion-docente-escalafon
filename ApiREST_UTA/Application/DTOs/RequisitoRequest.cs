namespace ApiREST_UTA.Application.DTOs
{
    public class RequisitoRequest
    {
        public string Cedula { get; set; } = null!;
        // Se ignora: el rol actual se toma de RRHH. Se mantiene por compatibilidad con clientes anteriores.
        public int RolActualId { get; set; }
    }
}
