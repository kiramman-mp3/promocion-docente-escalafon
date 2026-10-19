namespace ApiREST_UTA.Application.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string? Cedula { get; set; }
        public bool EsMiembroComision { get; set; }
        public bool EsPresidenteComision { get; set; }
        public string? CargoComision { get; set; }
    }
}
