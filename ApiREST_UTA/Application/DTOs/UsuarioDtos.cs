namespace ApiREST_UTA.Application.DTOs
{
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Correo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public string? Cedula { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearUsuarioRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Cedula { get; set; }   // Obligatoria para docentes
        public string Contrasena { get; set; } = string.Empty;
    }

    public class ActualizarUsuarioRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class CambiarContrasenaRequest
    {
        public string Contrasena { get; set; } = string.Empty;
    }
}
