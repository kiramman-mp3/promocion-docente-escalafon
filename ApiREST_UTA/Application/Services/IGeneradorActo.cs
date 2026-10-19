using ApiREST_UTA.Application.Services.Promocion;
using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Application.Services
{
    // Datos de la resolución de promoción (acto administrativo, CES art. 77 l)
    public class DatosActo
    {
        public SolicitudPromocion Solicitud { get; set; } = null!;
        public string NombreDocente { get; set; } = null!;
        public DictamenPromocion? Dictamen { get; set; }
        public List<VotoComision> Votos { get; set; } = new();
        public string? Presidente { get; set; }

        // Si la resolución viene de una apelación aceptada (COA arts. 217-218)
        public string? ResolucionApelacion { get; set; }
        public string? MotivacionApelacion { get; set; }
    }

    public interface IGeneradorActo
    {
        byte[] Generar(DatosActo datos);
    }
}
