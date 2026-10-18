using ApiREST_UTA.Domain.Entities;

namespace ApiREST_UTA.Application.Services.Promocion
{
    public class DictamenPromocion
    {
        public string Cedula { get; set; } = null!;
        public DateTime FechaCorte { get; set; }
        public int RolActualId { get; set; }
        public string RolActual { get; set; } = null!;
        public int? RolObjetivoId { get; set; }
        public string? RolObjetivo { get; set; }
        public bool Promocionable { get; set; }
        public int? RequisitosId { get; set; }
        public string? Normativa { get; set; }
        public string? Resolucion { get; set; }
        public bool RequisitosPropuestos { get; set; }
        public bool CumpleTodo { get; set; }
        public List<ResultadoRequisito> Requisitos { get; set; } = new();
        public List<string> Alertas { get; set; } = new();

        public ResultadoRequisito? Requisito(string codigo) => Requisitos.FirstOrDefault(r => r.Codigo == codigo);
    }

    // Aplica las reglas en orden. Si el grado no tiene promoción (R1) no evalúa nada más.
    public class MotorPromocion
    {
        private static readonly IReglaPromocion[] Reglas =
        {
            new ReglaPermanencia(),
            new ReglaObras(),
            new ReglaEvaluacion(),
            new ReglaCapacitacion(),
            new ReglaProyectos(),
            new ReglaDireccionProyectos(),
            new ReglaTitulacion()
        };

        public DictamenPromocion Evaluar(ContextoPromocion c)
        {
            var dictamen = new DictamenPromocion
            {
                Cedula = c.Cedula,
                FechaCorte = c.FechaCorte,
                RolActualId = c.RolActual.Id,
                RolActual = c.RolActual.Nombre,
                RolObjetivoId = c.RolObjetivo?.Id,
                RolObjetivo = c.RolObjetivo?.Nombre,
                Promocionable = c.RolActual.Promocionable,
                RequisitosId = c.Requisitos?.Id,
                Normativa = c.Requisitos?.Normativa,
                Resolucion = c.Requisitos?.Resolucion,
                RequisitosPropuestos = c.Requisitos?.EsPropuesta ?? false,
                Alertas = AlertasEvaluacion(c.Evaluaciones)
            };

            dictamen.Requisitos.Add(new ReglaGradoPromocionable().Evaluar(c));
            if (!c.RolActual.Promocionable)
                return dictamen;

            if (c.Requisitos == null)
            {
                dictamen.Requisitos.Add(new ResultadoRequisito
                {
                    Codigo = "R0",
                    Nombre = "Requisitos vigentes",
                    Articulo = "CES art. 77; UTA art. 49",
                    Cumple = false,
                    Obtenido = "No hay requisitos vigentes para este grado",
                    Requerido = "Requisitos aprobados para la promoción",
                    Detalle = "El administrador debe registrar los requisitos aprobados por la universidad."
                });
                return dictamen;
            }

            foreach (var regla in Reglas)
            {
                var resultado = regla.Evaluar(c);
                if (resultado != null)
                    dictamen.Requisitos.Add(resultado);
            }

            dictamen.CumpleTodo = dictamen.Requisitos.All(r => r.Cumple);
            return dictamen;
        }

        // Reglamento de evaluación UTA art. 15 y CES art. 98: menos de 70 % en dos periodos consecutivos o en cuatro
        public static List<string> AlertasEvaluacion(List<EvaluacionDocente> evaluaciones)
        {
            var alertas = new List<string>();
            var ordenadas = evaluaciones.OrderBy(e => e.Periodo).ToList();
            var consecutivas = ordenadas.Zip(ordenadas.Skip(1)).Any(p => p.First.Puntaje < 70 && p.Second.Puntaje < 70);
            var bajas = ordenadas.Count(e => e.Puntaje < 70);

            if (consecutivas)
                alertas.Add("Tiene dos periodos consecutivos con evaluación menor a 70 %, causal de destitución (Reglamento de evaluación UTA art. 15; CES art. 98).");
            else if (bajas >= 4)
                alertas.Add("Tiene cuatro periodos con evaluación menor a 70 % en su carrera, causal de destitución (Reglamento de evaluación UTA art. 15; CES art. 98).");
            else if (bajas > 0)
                alertas.Add($"Tiene {(bajas == 1 ? "1 periodo" : $"{bajas} periodos")} con evaluación menor a 70 %. Dos consecutivos o cuatro en la carrera son causal de destitución.");
            return alertas;
        }
    }
}
