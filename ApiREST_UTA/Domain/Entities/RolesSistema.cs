namespace ApiREST_UTA.Domain.Entities
{
    // Roles de usuario del sistema (DITIC.dbo.Usuarios.Rol). No confundir con los roles del escalafón.
    public static class RolesSistema
    {
        public const string Administrador = "Administrador";
        public const string RRHH = "RRHH";
        public const string Docente = "Docente";

        // Para [Authorize(Roles = ...)]: quienes revisan y aprueban solicitudes
        public const string Revisores = Administrador + "," + RRHH;

        public static readonly string[] Todos = { Administrador, RRHH, Docente };
    }
}
