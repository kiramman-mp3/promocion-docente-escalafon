namespace ApiREST_UTA.Application.Services
{
    // Error esperado de una regla de negocio; el controlador lo traduce a la respuesta HTTP indicada.
    public class ReglaNegocioException : Exception
    {
        public int StatusCode { get; }

        public ReglaNegocioException(int statusCode, string mensaje) : base(mensaje)
        {
            StatusCode = statusCode;
        }

        public static ReglaNegocioException NoEncontrado(string mensaje) => new(StatusCodes.Status404NotFound, mensaje);
        public static ReglaNegocioException Invalido(string mensaje) => new(StatusCodes.Status400BadRequest, mensaje);
        public static ReglaNegocioException Conflicto(string mensaje) => new(StatusCodes.Status409Conflict, mensaje);
        public static ReglaNegocioException Prohibido(string mensaje) => new(StatusCodes.Status403Forbidden, mensaje);
    }
}
