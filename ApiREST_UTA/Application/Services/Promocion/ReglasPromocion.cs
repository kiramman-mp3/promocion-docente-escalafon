using ApiREST_UTA.Domain.Entities;
using System.Globalization;

namespace ApiREST_UTA.Application.Services.Promocion
{
    public interface IReglaPromocion
    {
        // null cuando el requisito no aplica (por ejemplo, un requisito opcional que la universidad no exige)
        ResultadoRequisito? Evaluar(ContextoPromocion c);
    }

    internal static class Formato
    {
        private static readonly CultureInfo Es = new("es-EC");
        public static string Numero(decimal n) => n.ToString("0.##", Es);
        public static string Plural(int n, string singular, string plural) => $"{n} {(n == 1 ? singular : plural)}";
    }

    // R1. CES art. 76 y UTA art. 48: el grado 5 (Agregado 3) no se promociona y el 8 es el último
    public class ReglaGradoPromocionable : IReglaPromocion
    {
        public const string Codigo = "R1";

        public ResultadoRequisito Evaluar(ContextoPromocion c)
        {
            var detalle = c.RolActual.Promocionable
                ? $"Puede promoverse a {c.RolObjetivo?.Nombre}."
                : c.RolActual.Id == 5
                    ? "Agregado 3 no se promociona: Principal 1 se obtiene ganando un concurso público de méritos y oposición (CES arts. 59 y 76)."
                    : "Es el último grado del escalafón.";

            return new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Grado con promoción al siguiente",
                Articulo = "CES art. 76; UTA art. 48",
                Cumple = c.RolActual.Promocionable,
                Obtenido = c.RolActual.Nombre,
                Requerido = "Un grado distinto de Agregado 3 y Principal 3",
                Detalle = detalle
            };
        }
    }

    // R2. Permanencia mínima en el grado (CES arts. 63, 76 a y 77 a; UTA art. 48 a) y experiencia acumulada
    // del personal que ingresó antes del 07-11-2012 (UTA transitoria quinta; CES transitoria décima primera)
    public class ReglaPermanencia : IReglaPromocion
    {
        public const string Codigo = "R2";

        public ResultadoRequisito Evaluar(ContextoPromocion c)
        {
            var anios = c.RolActual.PermanenciaAnios ?? 0;
            var mesesGrado = Fechas.MesesCompletos(c.FechaInicioGrado, c.FechaCorte);
            var resultado = new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Tiempo en el grado actual",
                Articulo = "CES arts. 63, 76 a y 77 a; UTA art. 48 a",
                Cumple = mesesGrado >= anios * 12,
                Obtenido = $"{Fechas.Duracion(mesesGrado)} como {c.RolActual.Nombre}",
                Requerido = Formato.Plural(anios, "año", "años"),
                Detalle = $"Desde el {c.FechaInicioGrado:dd/MM/yyyy}."
            };

            var t = c.Trayectoria;
            if (!resultado.Cumple && t != null && t.IngresoAntesDe2012)
            {
                var acumulado = Fechas.MesesCompletos(t.FechaIngresoTitular, c.FechaCorte) + t.MesesNoTitular + t.MesesTecnicoDocente;
                var requeridoAcumulado = c.Roles.Where(r => r.Id <= c.RolActual.Id).Sum(r => r.PermanenciaAnios ?? 0) * 12;
                if (acumulado >= requeridoAcumulado)
                {
                    resultado.Cumple = true;
                    resultado.Exento = true;
                    resultado.Obtenido = $"{Fechas.Duracion(acumulado)} de experiencia acumulada";
                    resultado.Requerido = $"{Fechas.Duracion(requeridoAcumulado)} (suma de las permanencias hasta {c.RolActual.Nombre})";
                    resultado.Detalle = "Ingresó antes del 07-11-2012: la experiencia es acumulativa desde el inicio de su carrera como titular " +
                        "e incluye su tiempo como no titular y técnico docente (UTA, transitoria quinta).";
                }
            }

            return resultado;
        }
    }

    // R3. Obras de relevancia (CES arts. 76 b y 78-87; UTA art. 48 b). Excepción de publicar en el grado:
    // autoridad del art. 77 f durante dos años o más.
    public class ReglaObras : IReglaPromocion
    {
        public const string Codigo = "R3";

        public ResultadoRequisito? Evaluar(ContextoPromocion c)
        {
            var r = c.Requisitos;
            if (r == null) return null;

            var evaluadas = c.Obras.Select(o => (Obra: o, Criterio: o.EvaluarRelevancia())).ToList();
            var relevantes = evaluadas.Where(x => x.Criterio.Relevante).Select(x => x.Obra).ToList();
            var enGrado = relevantes.Count(o => o.FechaPublicacion >= c.FechaInicioGrado);
            var exentoEnGrado = c.MesesAutoridadEnGrado(soloArticulo77f: true) >= 24;
            var cumpleTotal = relevantes.Count >= r.ObrasTotales;
            var cumpleEnGrado = enGrado >= r.ObrasEnGrado || exentoEnGrado;

            var detalles = new List<string>();
            var noRelevantes = evaluadas.Where(x => !x.Criterio.Relevante).ToList();
            if (noRelevantes.Count > 0)
                detalles.Add($"No cuentan {Formato.Plural(noRelevantes.Count, "obra", "obras")}: " +
                    string.Join("; ", noRelevantes.Select(x => $"\"{x.Obra.Titulo ?? x.Obra.NombreRevista}\" {x.Criterio.Motivo}")) + ".");
            if (exentoEnGrado && enGrado < r.ObrasEnGrado)
                detalles.Add("Exento de publicar en el grado por haber sido autoridad dos años o más (CES art. 77 f).");

            return new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Obras de relevancia",
                Articulo = "CES arts. 76 b y 78-87; UTA art. 48 b",
                Cumple = cumpleTotal && cumpleEnGrado,
                Exento = exentoEnGrado && enGrado < r.ObrasEnGrado,
                Obtenido = $"{Formato.Plural(relevantes.Count, "obra relevante", "obras relevantes")}, {enGrado} en el grado actual",
                Requerido = $"{r.ObrasTotales} en total y al menos {r.ObrasEnGrado} en el grado actual",
                Detalle = string.Join(" ", detalles)
            };
        }
    }

    // R4. Evaluación de desempeño (CES arts. 76 c y 77 f; UTA art. 48 c)
    public class ReglaEvaluacion : IReglaPromocion
    {
        public const string Codigo = "R4";

        public ResultadoRequisito? Evaluar(ContextoPromocion c)
        {
            var r = c.Requisitos;
            if (r == null) return null;

            var resultado = new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Evaluación de desempeño",
                Articulo = "CES arts. 76 c y 77 f; UTA art. 48 c",
                Requerido = $"Promedio de {Formato.Numero(r.EvaluacionMinima)} % en las {r.EvaluacionPeriodos} últimas evaluaciones en las que ejerció la docencia"
            };

            List<EvaluacionDocente> consideradas;
            if (c.MesesAutoridadEnGrado(soloArticulo77f: true) > 0 && c.Evaluaciones.Count > 0)
            {
                // Art. 77 f: para quien fue rector, vicerrector o autoridad del SES se consideran los últimos 2 años evaluados
                var ultima = c.Evaluaciones.Max(e => e.Periodo);
                consideradas = c.Evaluaciones.Where(e => e.Periodo > ultima.AddYears(-2)).OrderByDescending(e => e.Periodo).ToList();
                resultado.Exento = true;
                resultado.Requerido = $"Promedio de {Formato.Numero(r.EvaluacionMinima)} % en los últimos 2 años evaluados (art. 77 f)";
                resultado.Cumple = consideradas.Count > 0 && consideradas.Average(e => e.Puntaje) >= r.EvaluacionMinima;
            }
            else
            {
                consideradas = c.Evaluaciones.Where(e => e.EjercioDocencia)
                    .OrderByDescending(e => e.Periodo).Take(r.EvaluacionPeriodos).ToList();
                resultado.Cumple = consideradas.Count == r.EvaluacionPeriodos && consideradas.Average(e => e.Puntaje) >= r.EvaluacionMinima;
            }

            var sinDocencia = c.Evaluaciones.Count(e => !e.EjercioDocencia);
            resultado.Obtenido = consideradas.Count == 0
                ? "Sin evaluaciones que se puedan considerar"
                : $"Promedio de {Formato.Numero(Math.Round(consideradas.Average(e => e.Puntaje), 2))} % en {Formato.Plural(consideradas.Count, "periodo", "periodos")}";
            resultado.Detalle = (consideradas.Count > 0
                    ? "Periodos: " + string.Join(", ", consideradas.Select(e => e.PeriodoAcademico ?? e.Periodo.ToString("MM/yyyy"))) + "."
                    : "")
                + (sinDocencia > 0 && !resultado.Exento ? $" No se cuentan {Formato.Plural(sinDocencia, "periodo", "periodos")} sin docencia." : "");
            return resultado;
        }
    }

    // R5. Actualización científica y pedagógica (CES arts. 76 d y 77 e-i; UTA art. 48 d)
    public class ReglaCapacitacion : IReglaPromocion
    {
        public const string Codigo = "R5";

        public ResultadoRequisito? Evaluar(ContextoPromocion c)
        {
            var r = c.Requisitos;
            if (r == null) return null;

            var desde = c.FechaCorte.AddYears(-r.CapacitacionAnios);
            var resultado = new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Actualización científica y pedagógica",
                Articulo = "CES arts. 76 d y 77 e-i; UTA art. 48 d"
            };

            // Art. 76 d y 77 f: autoridad dos años o más en el periodo de promoción queda exenta
            var mesesAutoridad77f = c.MesesAutoridadEnGrado(soloArticulo77f: true);
            var mesesOtraAutoridad = c.MesesAutoridadEnGrado(soloArticulo77f: false) - mesesAutoridad77f;
            if (mesesAutoridad77f >= 24 || mesesOtraAutoridad > 24)
            {
                resultado.Cumple = true;
                resultado.Exento = true;
                resultado.Obtenido = $"{Fechas.Duracion(mesesAutoridad77f + mesesOtraAutoridad)} como autoridad";
                resultado.Requerido = "No se exige";
                resultado.Detalle = "Exento por haberse desempeñado como autoridad en el Sistema de Educación Superior por más de dos años (CES arts. 76 d y 77 f).";
                return resultado;
            }

            var factor = 1m;
            var notas = new List<string>();
            var mesesAutoridadVentana = c.MesesAutoridadEnVentana(soloArticulo77f: true, desde);
            if (mesesAutoridadVentana > 0)
            {
                factor -= Math.Min(mesesAutoridadVentana, 36) / 36m;
                notas.Add($"Se reduce en proporción a {Fechas.Duracion(mesesAutoridadVentana)} como autoridad (art. 77 f).");
            }
            if (c.TuvoSabaticoAprobadoEnGrado)
            {
                factor *= 0.75m;
                notas.Add("Se reduce 25 % por el periodo sabático con informe aprobado (art. 77 e).");
            }
            resultado.Exento = factor < 1;

            var requerido = (int)Math.Ceiling(r.CapacitacionHoras * factor);
            var requeridoPedagogico = (int)Math.Ceiling(requerido * r.CapacitacionPctPedagogica / 100m);

            var cursos = c.Cursos.Where(x => x.FechaReferencia >= desde && x.FechaReferencia <= c.FechaCorte).ToList();
            var horasCursos = cursos.Sum(x => x.Horas);
            var horasPedagogicas = cursos.Where(x => x.CuentaComoPedagogica).Sum(x => x.Horas);

            // Equivalencias (art. 77 g-i): una sola facilitación del CES por promoción y máximo 2 meses en el CACES
            var equivalencias = c.Equivalencias.Where(e => e.Fecha >= desde && e.Fecha <= c.FechaCorte).ToList();
            var horasEquivalencias =
                equivalencias.Where(e => e.Tipo is Equivalencia.EvaluadorExternoConcurso or Equivalencia.ParEvaluadorProyectos).Sum(e => e.HorasCapacitacion)
                + equivalencias.Where(e => e.EsFacilitacionCES).Select(e => e.HorasCapacitacion).DefaultIfEmpty(0).Max()
                + Math.Min(equivalencias.Where(e => e.Tipo == Equivalencia.FacilitadorCACES).Sum(e => e.Meses ?? 0), 2) * 16;

            var total = horasCursos + horasEquivalencias;
            resultado.Cumple = total >= requerido && horasPedagogicas >= requeridoPedagogico;
            resultado.Obtenido = $"{total} h en los últimos {r.CapacitacionAnios} años ({horasPedagogicas} h pedagógicas o de congresos"
                + (horasEquivalencias > 0 ? $"; {horasEquivalencias} h por equivalencias" : "") + ")";
            resultado.Requerido = $"{requerido} h, de ellas {requeridoPedagogico} h pedagógicas o de congresos internacionales";
            resultado.Detalle = string.Join(" ", notas);
            return resultado;
        }
    }

    // R6. Participación en proyectos de investigación o vinculación (UTA art. 48 e.1)
    public class ReglaProyectos : IReglaPromocion
    {
        public const string Codigo = "R6";

        public ResultadoRequisito? Evaluar(ContextoPromocion c)
        {
            var r = c.Requisitos;
            if (r == null || r.ProyectosMeses <= 0) return null;

            var meses = c.Proyectos.Sum(p => p.Meses);
            return new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Participación en proyectos de investigación o vinculación",
                Articulo = "UTA art. 48 e.1; CES art. 76 e.1",
                Cumple = meses >= r.ProyectosMeses,
                Obtenido = $"{Formato.Plural(meses, "mes", "meses")} en {Formato.Plural(c.Proyectos.Count, "proyecto", "proyectos")}",
                Requerido = Formato.Plural(r.ProyectosMeses, "mes", "meses"),
                Detalle = c.Proyectos.Count > 0
                    ? $"Con la ponderación del CES art. 36 (dirección x2, codirección x1,5) equivalen a {Formato.Numero(c.Proyectos.Sum(p => p.MesesPonderados))} meses."
                    : ""
            };
        }
    }

    // R7. Dirección o codirección de proyectos (UTA art. 48 e.2)
    public class ReglaDireccionProyectos : IReglaPromocion
    {
        public const string Codigo = "R7";

        public ResultadoRequisito? Evaluar(ContextoPromocion c)
        {
            var r = c.Requisitos;
            if (r == null || !r.RequiereDireccionProyecto) return null;

            var dirigidos = c.Proyectos.Where(p => p.EsDireccion).ToList();
            return new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Dirección o codirección de proyectos",
                Articulo = "UTA art. 48 e.2; CES art. 76 e.2",
                Cumple = dirigidos.Count > 0,
                Obtenido = dirigidos.Count == 0
                    ? "Ningún proyecto dirigido"
                    : string.Join("; ", dirigidos.Select(p => $"{p.Rol} de \"{p.NombreProyecto}\"")),
                Requerido = "Al menos un proyecto dirigido o codirigido"
            };
        }
    }

    // R8. Dirección de trabajos de titulación (UTA art. 48 e.3) con las equivalencias de la transitoria quinta
    // (ingreso antes de 2012 con PhD) y del CES art. 77 j (3 evaluaciones académicas del CES = 1 dirección)
    public class ReglaTitulacion : IReglaPromocion
    {
        public const string Codigo = "R8";

        public ResultadoRequisito? Evaluar(ContextoPromocion c)
        {
            var r = c.Requisitos;
            if (r == null || r.TrabajosTitulacion <= 0) return null;

            var rangoMinimo = TrabajoTitulacion.Rango(r.TitulacionNivelMinimo);
            var equivalenciaPrevia = c.Trayectoria is { IngresoAntesDe2012: true, IngresoConPhD: true };

            var computables = 0;
            foreach (var t in c.TrabajosTitulacion)
            {
                if (!equivalenciaPrevia && TrabajoTitulacion.Rango(t.Nivel) < rangoMinimo)
                    continue;
                // CES art. 36 b.3: una tesis doctoral equivale a tres de maestría
                computables += r.TitulacionNivelMinimo == RequisitoPromocion.NivelMaestria && t.Nivel == RequisitoPromocion.NivelDoctorado ? 3 : 1;
            }

            var porEvaluacionesCES = c.Equivalencias.Count(e => e.Tipo == Equivalencia.EvaluacionAcademicaCES) / 3;
            var total = computables + porEvaluacionesCES;

            var nivel = r.TitulacionNivelMinimo switch
            {
                RequisitoPromocion.NivelMaestria => "de maestría o doctorado (1 doctorado = 3 maestrías)",
                RequisitoPromocion.NivelDoctorado => "de doctorado",
                _ => "de grado o posgrado"
            };
            var notas = new List<string>();
            if (porEvaluacionesCES > 0)
                notas.Add($"Incluye {porEvaluacionesCES} por evaluaciones académicas del CES (art. 77 j).");
            if (equivalenciaPrevia)
                notas.Add("Ingresó con PhD antes del 07-11-2012: las tesis de cualquier nivel son equivalentes (UTA, transitoria quinta).");

            return new ResultadoRequisito
            {
                Codigo = Codigo,
                Nombre = "Dirección de trabajos de titulación",
                Articulo = "UTA art. 48 e.3; CES arts. 36 b.3 y 77 j",
                Cumple = total >= r.TrabajosTitulacion,
                Exento = porEvaluacionesCES > 0 || equivalenciaPrevia,
                Obtenido = $"{total} computables de {Formato.Plural(c.TrabajosTitulacion.Count, "trabajo dirigido", "trabajos dirigidos")}",
                Requerido = $"{r.TrabajosTitulacion} {nivel}",
                Detalle = string.Join(" ", notas)
            };
        }
    }
}
