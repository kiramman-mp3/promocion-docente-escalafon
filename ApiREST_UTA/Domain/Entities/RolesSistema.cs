namespace ApiREST_UTA.Domain.Entities
{
    // Roles de usuario del sistema (DITIC.dbo.Usuarios.Rol). No confundir con los roles del escalafón.
    // La pertenencia a la Comisión Académica no es un rol: se define en DITIC.dbo.ComisionMiembros.
    public static class RolesSistema
    {
        public const string Administrador = "Administrador";
        public const string RRHH = "RRHH";

        // Autoridades sin carga docente que integran la Comisión Académica (Vicerrector, Director Financiero)
        public const string Autoridad = "Autoridad";
        public const string Docente = "Docente";

        // Para [Authorize(Roles = ...)]: quienes revisan documentos y gestionan el trámite (Talento Humano y administración)
        public const string Revisores = Administrador + "," + RRHH;

        public static readonly string[] Todos = { Administrador, RRHH, Autoridad, Docente };
    }
}
