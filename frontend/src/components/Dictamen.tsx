import type { Expediente } from "../types";
import { formatoFecha, formatoNumero, plural } from "../format";

interface Requisito {
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

export function Dictamen({ expediente }: { expediente: Expediente }) {
  const { tiempo, resultado, errorResultado } = expediente;

  if (!resultado) {
    return (
      <section className="dictamen" aria-labelledby="dictamen-titulo">
        <h2 id="dictamen-titulo" className="dictamen__veredicto">
          {errorResultado}
        </h2>
        <p className="dictamen__contexto">
          Cédula {tiempo.cedula}, {tiempo.rolActual} desde el {formatoFecha(tiempo.fechaInicio)}.
        </p>
      </section>
    );
  }

  const requisitos = requisitosDe(expediente);
  const faltantes = requisitos.filter((x) => !x.cumple).length;

  return (
    <section className="dictamen" aria-labelledby="dictamen-titulo">
      <p className="dictamen__contexto">
        Cédula {tiempo.cedula}, de {tiempo.rolActual} a {resultado.rolObjetivo}
      </p>
      <h2 id="dictamen-titulo" className={`dictamen__veredicto ${faltantes ? "" : "dictamen__veredicto--cumple"}`}>
        {faltantes
          ? `Le ${faltantes === 1 ? "falta 1 requisito" : `faltan ${faltantes} requisitos`} para ${resultado.rolObjetivo}`
          : `Cumple todos los requisitos para ${resultado.rolObjetivo}`}
      </h2>

      <ul className="requisitos">
        {requisitos.map((req) => (
          <li key={req.nombre} className={`requisito ${req.cumple ? "requisito--cumple" : "requisito--falta"}`}>
            <span className="requisito__marca" aria-hidden="true">
              {req.cumple ? (
                <svg viewBox="0 0 20 20"><path d="M5 10.5l3.2 3.2L15 7" /></svg>
              ) : (
                <svg viewBox="0 0 20 20"><path d="M6 6l8 8M14 6l-8 8" /></svg>
              )}
            </span>
            <span className="requisito__nombre">{req.nombre}</span>
            <span className="requisito__detalle">{req.detalle}</span>
            <span className="requisito__estado">{req.cumple ? "Cumple" : "No cumple"}</span>
          </li>
        ))}
      </ul>
    </section>
  );
}
