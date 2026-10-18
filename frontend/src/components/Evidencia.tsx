import { useId, useState, type KeyboardEvent, type ReactNode } from "react";
import { abrirPdf } from "../api";
import { formatoFecha, formatoMes, formatoNumero, plural } from "../format";
import type { EstadoRevision, Expediente, RevisionDocumento, TipoDocumento } from "../types";
import { ControlRevision } from "./ControlRevision";

// Con modoRevision, cada documento muestra su estado de revisión y (si es editable) los botones para revisarlo.
export interface ModoRevision {
  revisiones: RevisionDocumento[];
  editable: boolean;
  onRevisar: (tipo: TipoDocumento, documentoId: number, estado: EstadoRevision, observacion?: string) => Promise<void>;
}

function BotonPdf({ base64, etiqueta }: { base64: string; etiqueta: string }) {
  return (
    <button type="button" className="boton-pdf" onClick={() => abrirPdf(base64)} aria-label={`Ver PDF: ${etiqueta}`}>
      Ver PDF
    </button>
  );
}

function Vacio({ children }: { children: ReactNode }) {
  return <p className="evidencia__vacio">{children}</p>;
}

export function Evidencia({ expediente: e, modoRevision }: { expediente: Expediente; modoRevision?: ModoRevision }) {
  const base = useId();

  const revisionDe = (tipo: TipoDocumento, id: number) =>
    modoRevision?.revisiones.find((r) => r.tipoDocumento === tipo && r.documentoId === id);

  function celdaRevision(tipo: TipoDocumento, id: number, etiqueta: string) {
    if (!modoRevision) return null;
    return (
      <td className="tabla__revision">
        <ControlRevision
          revision={revisionDe(tipo, id)}
          editable={modoRevision.editable}
          etiqueta={etiqueta}
          onRevisar={(estado, observacion) => modoRevision.onRevisar(tipo, id, estado, observacion)}
        />
      </td>
    );
  }
  const columnaRevision = modoRevision ? <th scope="col">Revisión</th> : null;

  const documentos = {
    obras: e.obras.map((o) => ["Obra", o.id] as const),
    evaluaciones: e.evaluaciones.map((x) => ["Evaluacion", x.id] as const),
    cursos: e.cursos.map((c) => ["Curso", c.id] as const),
    investigaciones: e.investigaciones.map((i) => ["Investigacion", i.id] as const),
    otros: [["TiempoRol", e.tiempo.id] as const],
  };
  // En modo revisión la pestaña muestra cuántos documentos ya se revisaron
  const etiquetaCantidad = (clave: keyof typeof documentos) => {
    const docs = documentos[clave];
    if (!modoRevision) return clave === "otros" ? undefined : `${docs.length}`;
    const revisados = docs.filter(([tipo, id]) => revisionDe(tipo, id)).length;
    return `${revisados}/${docs.length}`;
  };

  const pestanas = [
    { id: "obras", nombre: "Obras" },
    { id: "evaluaciones", nombre: "Evaluaciones" },
    { id: "cursos", nombre: "Cursos" },
    { id: "investigaciones", nombre: "Investigaciones" },
    { id: "otros", nombre: "Otros méritos" },
  ] as const;
  const [activa, setActiva] = useState<(typeof pestanas)[number]["id"]>("obras");

  // Flechas izquierda/derecha para moverse entre pestañas (patrón ARIA tabs)
  function alTeclear(ev: KeyboardEvent<HTMLDivElement>) {
    if (ev.key !== "ArrowRight" && ev.key !== "ArrowLeft") return;
    const i = pestanas.findIndex((p) => p.id === activa);
    const siguiente = pestanas[(i + (ev.key === "ArrowRight" ? 1 : pestanas.length - 1)) % pestanas.length];
    setActiva(siguiente.id);
    document.getElementById(`${base}-tab-${siguiente.id}`)?.focus();
  }

  return (
    <section className="evidencia" aria-labelledby={`${base}-titulo`}>
      <h2 id={`${base}-titulo`} className="evidencia__titulo">
        Documentos del expediente
      </h2>

      <div className="pestanas" role="tablist" aria-label="Tipo de documento" onKeyDown={alTeclear}>
        {pestanas.map((p) => {
          const cantidad = etiquetaCantidad(p.id);
          return (
            <button
              key={p.id}
              id={`${base}-tab-${p.id}`}
              type="button"
              role="tab"
              aria-selected={activa === p.id}
              aria-controls={`${base}-panel`}
              tabIndex={activa === p.id ? 0 : -1}
              className="pestana"
              onClick={() => setActiva(p.id)}
            >
              {p.nombre}
              {cantidad !== undefined && <span className="pestana__cantidad">{cantidad}</span>}
            </button>
          );
        })}
      </div>

      <div id={`${base}-panel`} role="tabpanel" aria-labelledby={`${base}-tab-${activa}`} className="evidencia__panel">
        {activa === "obras" &&
          (e.obras.length ? (
            <table className="tabla">
              <thead>
                <tr><th scope="col">Revista</th><th scope="col">Publicación</th><th scope="col"><span className="sr-only">Documento</span></th>{columnaRevision}</tr>
              </thead>
              <tbody>
                {e.obras.map((o) => (
                  <tr key={o.id}>
                    <td>{o.nombreRevista}</td>
                    <td className="tabla__fecha">{formatoFecha(o.fechaPublicacion)}</td>
                    <td className="tabla__accion"><BotonPdf base64={o.pdfBase64} etiqueta={o.nombreRevista} /></td>
                    {celdaRevision("Obra", o.id, `la obra en ${o.nombreRevista}`)}
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DIDE no registra obras publicadas desde que el docente asumió su rol actual.</Vacio>
          ))}

        {activa === "evaluaciones" &&
          (e.evaluaciones.length ? (
            <table className="tabla">
              <thead>
                <tr><th scope="col">Periodo</th><th scope="col">Carrera</th><th scope="col" className="tabla__num">Puntaje</th><th scope="col"><span className="sr-only">Documento</span></th>{columnaRevision}</tr>
              </thead>
              <tbody>
                {e.evaluaciones.map((ev) => (
                  <tr key={ev.id}>
                    <td className="tabla__fecha">{formatoMes(ev.periodo)}</td>
                    <td>{ev.carrera}</td>
                    <td className="tabla__num">{formatoNumero(ev.puntaje)}</td>
                    <td className="tabla__accion"><BotonPdf base64={ev.pdf} etiqueta={`Evaluación ${formatoMes(ev.periodo)}`} /></td>
                    {celdaRevision("Evaluacion", ev.id, `la evaluación de ${formatoMes(ev.periodo)}`)}
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DEaDV no tiene evaluaciones docentes para esta cédula.</Vacio>
          ))}

        {activa === "cursos" &&
          (e.cursos.length ? (
            <table className="tabla">
              <thead>
                <tr><th scope="col">Universidad</th><th scope="col">Facultad</th><th scope="col">Fecha</th><th scope="col" className="tabla__num">Horas</th>{columnaRevision}</tr>
              </thead>
              <tbody>
                {e.cursos.map((c) => (
                  <tr key={c.id}>
                    <td>{c.universidad}</td>
                    <td>{c.facultad}</td>
                    <td className="tabla__fecha">{formatoFecha(c.fechaEvaluacion)}</td>
                    <td className="tabla__num">{formatoNumero(c.puntaje)}</td>
                    {celdaRevision("Curso", c.id, `el curso de ${c.facultad}`)}
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DEaDV no tiene cursos de capacitación para esta cédula.</Vacio>
          ))}

        {activa === "investigaciones" &&
          (e.investigaciones.length ? (
            <table className="tabla">
              <thead>
                <tr><th scope="col">Proyecto</th><th scope="col">Encargado</th><th scope="col" className="tabla__num">Horas</th><th scope="col"><span className="sr-only">Documento</span></th>{columnaRevision}</tr>
              </thead>
              <tbody>
                {e.investigaciones.map((inv) => (
                  <tr key={inv.id}>
                    <td>{inv.nombre}</td>
                    <td>{inv.encargado}</td>
                    <td className="tabla__num">{formatoNumero(inv.horas)}</td>
                    <td className="tabla__accion"><BotonPdf base64={inv.pdf} etiqueta={inv.nombre} /></td>
                    {celdaRevision("Investigacion", inv.id, `la investigación ${inv.nombre}`)}
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DIDE no registra investigaciones para esta cédula.</Vacio>
          ))}

        {activa === "otros" && (
          <dl className="meritos">
            <div><dt>Tesis doctorales dirigidas</dt><dd>{plural(e.tesisDirigidas, "tesis", "tesis")}</dd></div>
            <div><dt>Meses ponderados en proyectos</dt><dd>{plural(e.mesesPonderados, "mes", "meses")}</dd></div>
            <div><dt>Capacitación impartida</dt><dd>{plural(e.horasImpartidas, "hora")}</dd></div>
            <div><dt>Obra en idioma extranjero</dt><dd>{e.tieneObraEnOtroIdioma ? "Sí" : "No"}</dd></div>
            <div className="meritos__pdf">
              <dt>Acción de personal ({e.tiempo.rolActual})</dt>
              <dd>
                <BotonPdf base64={e.tiempo.pdfBase64} etiqueta="Acción de personal" />
                {modoRevision && (
                  <ControlRevision
                    revision={revisionDe("TiempoRol", e.tiempo.id)}
                    editable={modoRevision.editable}
                    etiqueta="la acción de personal"
                    onRevisar={(estado, obs) => modoRevision.onRevisar("TiempoRol", e.tiempo.id, estado, obs)}
                  />
                )}
              </dd>
            </div>
          </dl>
        )}
      </div>
    </section>
  );
}
