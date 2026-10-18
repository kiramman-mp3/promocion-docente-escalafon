import { formatoFecha, formatoNumero, plural } from "./format";
import type { Expediente } from "./types";

export interface Requisito {
  nombre: string;
  cumple: boolean;
  detalle: string;
}

export function requisitosDe(e: Expediente): Requisito[] {
  const r = e.resultado!;
  const promedio = e.evaluaciones.length
    ? e.evaluaciones.reduce((s, x) => s + x.puntaje, 0) / e.evaluaciones.length
    : 0;
  const horasCursos = e.cursos.reduce((s, c) => s + c.puntaje, 0);
  const horasInvestigacion = e.investigaciones.reduce((s, i) => s + i.horas, 0);

  return [
    {
      nombre: "Tiempo en el rol actual",
      cumple: r.cumpleExperiencia,
      detalle: `${plural(e.tiempo.aniosEnRol, "año")} desde el ${formatoFecha(e.tiempo.fechaInicio)}`,
    },
    {
      nombre: "Obras publicadas con filiación UTA",
      cumple: r.cumpleObras,
      detalle: `${plural(e.obras.length, "obra")} desde que asumió el rol`,
    },
    {
      nombre: "Evaluación docente",
      cumple: r.cumpleEvaluacion,
      detalle: e.evaluaciones.length
        ? `Promedio de ${formatoNumero(promedio)} en ${plural(e.evaluaciones.length, "periodo")} (se piden al menos 4)`
        : "Sin evaluaciones registradas",
    },
    {
      nombre: "Capacitación recibida",
      cumple: r.cumpleCapacitacion,
      detalle: `${plural(horasCursos, "hora")} en ${plural(e.cursos.length, "curso")}`,
    },
    {
      nombre: "Participación en investigación",
      cumple: r.cumpleInvestigacion,
      detalle: `${plural(horasInvestigacion, "hora")} en ${plural(e.investigaciones.length, "proyecto")}`,
    },
    {
      nombre: "Tesis doctorales dirigidas",
      cumple: r.cumpleTesisDoctorado,
      detalle:
        e.rolActualId + 1 < 6
          ? "No se exige para este rol"
          : `${plural(e.tesisDirigidas, "tesis", "tesis")} registradas en DAC`,
    },
  ];
}
