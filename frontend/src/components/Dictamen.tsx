import { formatoFecha } from "../format";
import type { Dictamen as DatosDictamen } from "../types";

interface Props {
  dictamen: DatosDictamen | null;
  error?: string | null;
  // Dentro de una solicitud el veredicto es un apoyo, no el titular de la página
  secundario?: boolean;
}

function titulo(d: DatosDictamen) {
  if (!d.promocionable) {
    return d.rolActualId === 5
      ? "Agregado 3 no tiene promoción: Principal 1 se obtiene por concurso"
      : `${d.rolActual} es el último grado del escalafón`;
  }
  if (d.requisitosId == null) return `Aún no hay requisitos vigentes para ascender a ${d.rolObjetivo}`;
  const faltan = d.requisitos.filter((r) => !r.cumple).length;
  return faltan === 0
    ? `Cumple todos los requisitos para ${d.rolObjetivo}`
    : `Le ${faltan === 1 ? "falta 1 requisito" : `faltan ${faltan} requisitos`} para ${d.rolObjetivo}`;
}

export function Dictamen({ dictamen, error, secundario = false }: Props) {
  if (!dictamen) {
    return (
      <section className="dictamen" aria-labelledby="dictamen-titulo">
        <h2 id="dictamen-titulo" className="dictamen__veredicto">
          {error ?? "No se pudo calcular el dictamen."}
        </h2>
      </section>
    );
  }

  const cumple = dictamen.promocionable && dictamen.cumpleTodo;
  // R1 (grado con promoción) solo se muestra cuando bloquea
  const requisitos = dictamen.requisitos.filter((r) => r.codigo !== "R1" || !r.cumple);

  return (
    <section className={`dictamen ${secundario ? "dictamen--secundario" : ""}`} aria-labelledby="dictamen-titulo">
      <p className="dictamen__contexto">
        Cédula {dictamen.cedula}, {dictamen.rolActual}
        {dictamen.rolObjetivo ? ` a ${dictamen.rolObjetivo}` : ""}. Evaluado al {formatoFecha(dictamen.fechaCorte)}.
      </p>
      <h2 id="dictamen-titulo" className={`dictamen__veredicto ${cumple ? "dictamen__veredicto--cumple" : ""}`}>
        {titulo(dictamen)}
      </h2>

      {dictamen.requisitosPropuestos && (
        <p className="aviso-norma">
          Requisitos propuestos mientras la UTA expide su normativa interna de promoción (transitoria cuarta, 0153-HCU-2026).
        </p>
      )}

      {dictamen.alertas.map((a) => (
        <p key={a} className="alerta">
          {a}
        </p>
      ))}

      <ul className="requisitos">
        {requisitos.map((req) => (
          <li key={req.codigo} className={`requisito ${req.cumple ? "requisito--cumple" : "requisito--falta"}`}>
            <span className="requisito__marca" aria-hidden="true">
              {req.cumple ? (
                <svg viewBox="0 0 20 20"><path d="M5 10.5l3.2 3.2L15 7" /></svg>
              ) : (
                <svg viewBox="0 0 20 20"><path d="M6 6l8 8M14 6l-8 8" /></svg>
              )}
            </span>
            <span className="requisito__nombre">
              {req.nombre}
              <span className="requisito__articulo">{req.articulo}</span>
            </span>
            <span className="requisito__detalle">
              <span className="requisito__valores">
                {req.obtenido}
                <span className="requisito__requerido">Se exige: {req.requerido}</span>
              </span>
              {req.detalle && <span className="requisito__nota">{req.detalle}</span>}
            </span>
            <span className="requisito__estado">
              {req.cumple ? (req.exento ? "Cumple con excepción" : "Cumple") : "No cumple"}
            </span>
          </li>
        ))}
      </ul>

      {dictamen.normativa && <p className="dictamen__norma">Base legal: {dictamen.normativa}.</p>}
    </section>
  );
}
