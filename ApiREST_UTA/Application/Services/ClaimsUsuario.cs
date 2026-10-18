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

        // Revisores consultan cualquier cédula; un docente solo la suya
        public static bool PuedeConsultar(this ClaimsPrincipal usuario, string? cedula) =>
            usuario.EsRevisor() || (!string.IsNullOrWhiteSpace(cedula) && usuario.CedulaPropia() == cedula.Trim());
    }
}
