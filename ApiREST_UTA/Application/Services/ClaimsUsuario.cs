using ApiREST_UTA.Domain.Entities;
using System.Security.Claims;

namespace ApiREST_UTA.Application.Services
{
    public static class ClaimsUsuario
    {
        // Claim con la cédula del docente que inició sesión
        public const string Cedula = "cedula";

        public static int Id(this ClaimsPrincipal usuario) =>
            int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public static string? CedulaPropia(this ClaimsPrincipal usuario) =>
            usuario.FindFirstValue(Cedula);

        public static bool EsRevisor(this ClaimsPrincipal usuario) =>
            usuario.IsInRole(RolesSistema.Administrador) || usuario.IsInRole(RolesSistema.RRHH);

        // Claim que se agrega al token cuando el usuario es miembro vigente de la Comisión Académica (UTA art. 47)
        public const string Comision = "comision";

        public static bool EsMiembroComision(this ClaimsPrincipal usuario) =>
            usuario.FindFirstValue(Comision) == "true";

        // Revisores y miembros de la Comisión consultan cualquier cédula; un docente solo la suya
        public static bool PuedeConsultar(this ClaimsPrincipal usuario, string? cedula) =>
            usuario.EsRevisor() || usuario.EsMiembroComision()
            || (!string.IsNullOrWhiteSpace(cedula) && usuario.CedulaPropia() == cedula.Trim());

        // La Comisión ve las solicitudes que ya le fueron enviadas; el docente, las suyas
        public static bool PuedeVerSolicitud(this ClaimsPrincipal usuario, SolicitudPromocion solicitud) =>
            usuario.EsRevisor()
            || usuario.CedulaPropia() == solicitud.Cedula
            || (usuario.EsMiembroComision() && solicitud.LlegoAComision);
    }
}
