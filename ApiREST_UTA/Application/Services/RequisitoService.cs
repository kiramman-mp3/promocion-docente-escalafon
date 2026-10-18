using ApiREST_UTA.Application.DTOs;
using ApiREST_UTA.Domain.Entities;
using ApiREST_UTA.Domain.Interfaces;
using System.Security.Claims;

namespace ApiREST_UTA.Application.Services
{
    // Administración de los requisitos de promoción vigentes (CES arts. 76-77, UTA art. 48)
    public class RequisitoService
    {
        // Mínimos que fija el CES art. 76 y que la universidad no puede reducir (art. 77 b)
        public const decimal EvaluacionMinimaLegal = 75;
        public const int EvaluacionPeriodosLegal = 4;
        public const int CapacitacionHorasLegal = 96;
        public const int CapacitacionAniosLegal = 3;
        public const decimal CapacitacionPctPedagogicaLegal = 25;

        private readonly IRequisitoRepository _requisitos;
        private readonly IRolRepository _roles;

        public RequisitoService(IRequisitoRepository requisitos, IRolRepository roles)
        {
            _requisitos = requisitos;
            _roles = roles;
        }

        public async Task<List<RequisitoPromocionResponse>> ListarAsync(bool incluirHistorico)
        {
            var roles = await _roles.ListarAsync();
            var lista = await _requisitos.ListarAsync(incluirHistorico);
            return lista.Select(r => AResponse(r, roles)).ToList();
        }

        public async Task<RequisitoPromocionResponse> CrearAsync(RequisitoPromocionRequest request, ClaimsPrincipal admin)
        {
            var roles = await _roles.ListarAsync();
            var origen = roles.FirstOrDefault(r => r.Id == request.GradoOrigen)
                ?? throw ReglaNegocioException.Invalido("El grado de origen no existe en el escalafón.");
            if (!origen.Promocionable)
                throw ReglaNegocioException.Invalido(
                    $"{origen.Nombre} no tiene promoción al grado siguiente (CES art. 76): se accede por concurso o es el último grado.");

            ValidarMinimosLegales(request);

            var abierto = await _requisitos.ObtenerAbiertoAsync(request.GradoOrigen);
            if (abierto != null && request.VigenteDesde.Date <= abierto.VigenteDesde.Date)
                throw ReglaNegocioException.Invalido(
                    $"La nueva vigencia debe empezar después del {abierto.VigenteDesde:dd/MM/yyyy}, fecha del requisito actual.");

            await ValidarCrecientesAsync(request, roles);

            var nuevo = new RequisitoPromocion
            {
                GradoOrigen = request.GradoOrigen,
                GradoDestino = request.GradoOrigen + 1,
                Normativa = request.Normativa.Trim(),
                Resolucion = request.Resolucion.Trim(),
                EsPropuesta = request.EsPropuesta,
                VigenteDesde = request.VigenteDesde.Date,
                ObrasTotales = request.ObrasTotales,
                ObrasEnGrado = request.ObrasEnGrado,
                EvaluacionMinima = request.EvaluacionMinima,
                EvaluacionPeriodos = request.EvaluacionPeriodos,
                CapacitacionHoras = request.CapacitacionHoras,
                CapacitacionAnios = request.CapacitacionAnios,
                CapacitacionPctPedagogica = request.CapacitacionPctPedagogica,
                ProyectosMeses = request.ProyectosMeses,
                RequiereDireccionProyecto = request.RequiereDireccionProyecto,
                TrabajosTitulacion = request.TrabajosTitulacion,
                TitulacionNivelMinimo = request.TitulacionNivelMinimo,
                CreadoPorId = admin.Id()
            };
            nuevo.Id = await _requisitos.ReemplazarAsync(nuevo);

            return AResponse((await _requisitos.ObtenerPorIdAsync(nuevo.Id))!, roles);
        }

        private static void ValidarMinimosLegales(RequisitoPromocionRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Normativa) || string.IsNullOrWhiteSpace(r.Resolucion))
                throw ReglaNegocioException.Invalido("Indica la normativa y la resolución que respaldan los requisitos.");
            if (r.EvaluacionMinima < EvaluacionMinimaLegal || r.EvaluacionMinima > 100)
                throw ReglaNegocioException.Invalido("La evaluación mínima debe estar entre 75 % y 100 % (CES art. 76 c).");
            if (r.EvaluacionPeriodos != EvaluacionPeriodosLegal)
                throw ReglaNegocioException.Invalido("Se consideran las 4 últimas evaluaciones de desempeño (CES art. 76 c).");
            if (r.CapacitacionHoras < CapacitacionHorasLegal)
                throw ReglaNegocioException.Invalido("La capacitación mínima es de 96 horas (CES art. 76 d).");
            if (r.CapacitacionAnios != CapacitacionAniosLegal)
                throw ReglaNegocioException.Invalido("Las horas de capacitación se cuentan en los últimos 3 años (CES art. 76 d).");
            if (r.CapacitacionPctPedagogica < CapacitacionPctPedagogicaLegal || r.CapacitacionPctPedagogica > 100)
                throw ReglaNegocioException.Invalido("Al menos el 25 % de las horas deben ser pedagógicas o de congresos internacionales (CES art. 76 d).");
            if (r.ObrasEnGrado < 1)
                throw ReglaNegocioException.Invalido("Al menos una obra debe publicarse durante el grado actual (CES art. 76 b).");
            if (r.ObrasTotales < r.ObrasEnGrado)
                throw ReglaNegocioException.Invalido("Las obras totales no pueden ser menos que las exigidas en el grado.");
            if (r.ProyectosMeses < 0 || r.TrabajosTitulacion < 0)
                throw ReglaNegocioException.Invalido("Los meses de proyectos y los trabajos de titulación no pueden ser negativos.");
            if (!RequisitoPromocion.NivelesTitulacion.Contains(r.TitulacionNivelMinimo))
                throw ReglaNegocioException.Invalido("El nivel mínimo de titulación debe ser Grado, Maestria o Doctorado.");
        }

        // CES art. 76: los requisitos entre dos grados consecutivos son crecientes (se admite igualdad)
        private async Task ValidarCrecientesAsync(RequisitoPromocionRequest r, List<Rol> roles)
        {
            var anterior = roles.Any(x => x.Id == r.GradoOrigen - 1 && x.Promocionable)
                ? await _requisitos.ObtenerAbiertoAsync(r.GradoOrigen - 1) : null;
            var siguiente = roles.Any(x => x.Id == r.GradoOrigen + 1 && x.Promocionable)
                ? await _requisitos.ObtenerAbiertoAsync(r.GradoOrigen + 1) : null;

            void Comparar(string nombre, int valor, Func<RequisitoPromocion, int> campo)
            {
                if (anterior != null && valor < campo(anterior))
                    throw ReglaNegocioException.Invalido(
                        $"{nombre}: no puede ser menor que el del grado anterior ({campo(anterior)}). Los requisitos son crecientes (CES art. 76).");
                if (siguiente != null && valor > campo(siguiente))
                    throw ReglaNegocioException.Invalido(
                        $"{nombre}: no puede superar el del grado siguiente ({campo(siguiente)}). Los requisitos son crecientes (CES art. 76).");
            }

            Comparar("Obras totales", r.ObrasTotales, x => x.ObrasTotales);
            Comparar("Meses en proyectos", r.ProyectosMeses, x => x.ProyectosMeses);
            Comparar("Trabajos de titulación", r.TrabajosTitulacion, x => x.TrabajosTitulacion);
        }

        public static RequisitoPromocionResponse AResponse(RequisitoPromocion r, List<Rol> roles) => new()
        {
            Id = r.Id,
            GradoOrigen = r.GradoOrigen,
            RolOrigen = roles.FirstOrDefault(x => x.Id == r.GradoOrigen)?.Nombre ?? $"Grado {r.GradoOrigen}",
            GradoDestino = r.GradoDestino,
            RolDestino = roles.FirstOrDefault(x => x.Id == r.GradoDestino)?.Nombre ?? $"Grado {r.GradoDestino}",
            Normativa = r.Normativa,
            Resolucion = r.Resolucion,
            EsPropuesta = r.EsPropuesta,
            VigenteDesde = r.VigenteDesde,
            VigenteHasta = r.VigenteHasta,
            Vigente = r.VigenteEn(DateTime.Today),
            ObrasTotales = r.ObrasTotales,
            ObrasEnGrado = r.ObrasEnGrado,
            EvaluacionMinima = r.EvaluacionMinima,
            EvaluacionPeriodos = r.EvaluacionPeriodos,
            CapacitacionHoras = r.CapacitacionHoras,
            CapacitacionAnios = r.CapacitacionAnios,
            CapacitacionPctPedagogica = r.CapacitacionPctPedagogica,
            ProyectosMeses = r.ProyectosMeses,
            RequiereDireccionProyecto = r.RequiereDireccionProyecto,
            TrabajosTitulacion = r.TrabajosTitulacion,
            TitulacionNivelMinimo = r.TitulacionNivelMinimo
        };
    }
}
