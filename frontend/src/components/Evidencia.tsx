import { useId, useState, type KeyboardEvent, type ReactNode } from "react";
import { abrirPdf } from "../api";
import { formatoFecha, formatoMes, formatoNumero, nombreTipoCapacitacion, nombreTipoObra, plural } from "../format";
import type { Expediente } from "../types";

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

const PESTANAS = [
  { id: "obras", nombre: "Obras" },
  { id: "evaluaciones", nombre: "Evaluaciones" },
  { id: "capacitacion", nombre: "Capacitación" },
  { id: "proyectos", nombre: "Proyectos" },
  { id: "titulacion", nombre: "Titulación" },
  { id: "investigaciones", nombre: "Investigaciones" },
  { id: "otros", nombre: "Trayectoria" },
] as const;

type Pestana = (typeof PESTANAS)[number]["id"];

export function Evidencia({ expediente: e }: { expediente: Expediente }) {
  const base = useId();
  const [activa, setActiva] = useState<Pestana>("obras");

  const cantidades: Partial<Record<Pestana, number>> = {
    obras: e.obras.length,
    evaluaciones: e.evaluaciones.length,
    capacitacion: e.cursos.length + e.equivalencias.length,
    proyectos: e.proyectos.length,
    titulacion: e.trabajosTitulacion.length,
    investigaciones: e.investigaciones.length,
  };

  // Flechas izquierda/derecha para moverse entre pestañas (patrón ARIA tabs)
  function alTeclear(ev: KeyboardEvent<HTMLDivElement>) {
    if (ev.key !== "ArrowRight" && ev.key !== "ArrowLeft") return;
    const i = PESTANAS.findIndex((p) => p.id === activa);
    const siguiente = PESTANAS[(i + (ev.key === "ArrowRight" ? 1 : PESTANAS.length - 1)) % PESTANAS.length];
    setActiva(siguiente.id);
    document.getElementById(`${base}-tab-${siguiente.id}`)?.focus();
  }

  return (
    <section className="evidencia" aria-labelledby={`${base}-titulo`}>
      <h2 id={`${base}-titulo`} className="evidencia__titulo">
        Documentos del expediente
      </h2>

      <div className="pestanas" role="tablist" aria-label="Tipo de documento" onKeyDown={alTeclear}>
        {PESTANAS.map((p) => (
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
            {cantidades[p.id] !== undefined && <span className="pestana__cantidad">{cantidades[p.id]}</span>}
          </button>
        ))}
      </div>

      <div id={`${base}-panel`} role="tabpanel" aria-labelledby={`${base}-tab-${activa}`} className="evidencia__panel">
        {activa === "obras" &&
          (e.obras.length ? (
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Obra</th>
                  <th scope="col">Tipo</th>
                  <th scope="col">Publicación</th>
                  <th scope="col">Relevancia</th>
                  <th scope="col"><span className="sr-only">Documento</span></th>
                </tr>
              </thead>
              <tbody>
                {e.obras.map((o) => (
                  <tr key={o.id}>
                    <td>
                      {o.titulo ?? o.nombreRevista}
                      <span className="tabla__secundario">
                        {o.nombreRevista}
                        {o.baseIndexacion ? `, ${o.baseIndexacion}` : ""}
                        {o.idioma !== "Español" ? `, en ${o.idioma.toLowerCase()}` : ""}
                      </span>
                    </td>
                    <td>{nombreTipoObra(o.tipoObra)}</td>
                    <td className="tabla__fecha">{formatoFecha(o.fechaPublicacion)}</td>
                    <td>
                      {o.esRelevante ? (
                        <span className="marca-estado marca-estado--ok">Relevante</span>
                      ) : (
                        <span className="marca-estado marca-estado--no" title={o.motivoNoRelevante ?? ""}>
                          No cuenta: {o.motivoNoRelevante}
                        </span>
                      )}
                    </td>
                    <td className="tabla__accion"><BotonPdf base64={o.pdfBase64} etiqueta={o.titulo ?? o.nombreRevista} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DIDE no registra obras para esta cédula.</Vacio>
          ))}

        {activa === "evaluaciones" &&
          (e.evaluaciones.length ? (
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Periodo</th>
                  <th scope="col">Carrera</th>
                  <th scope="col" className="tabla__num">Resultado</th>
                  <th scope="col"><span className="sr-only">Documento</span></th>
                </tr>
              </thead>
              <tbody>
                {e.evaluaciones.map((ev) => (
                  <tr key={ev.id} className={ev.ejercioDocencia ? "" : "tabla__fila--tenue"}>
                    <td className="tabla__fecha">
                      {ev.periodoAcademico ?? formatoMes(ev.periodo)}
                      {!ev.ejercioDocencia && <span className="tabla__secundario">Sin docencia: no cuenta para el art. 76 c</span>}
                    </td>
                    <td>{ev.carrera}</td>
                    <td className={`tabla__num ${ev.puntaje < 70 ? "tabla__num--bajo" : ""}`}>{formatoNumero(ev.puntaje)} %</td>
                    <td className="tabla__accion"><BotonPdf base64={ev.pdf} etiqueta={`Evaluación ${ev.periodoAcademico ?? ""}`} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DEaDV no tiene evaluaciones docentes para esta cédula.</Vacio>
          ))}

        {activa === "capacitacion" &&
          (e.cursos.length || e.equivalencias.length ? (
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Curso o equivalencia</th>
                  <th scope="col">Tipo</th>
                  <th scope="col">Fecha</th>
                  <th scope="col" className="tabla__num">Horas</th>
                </tr>
              </thead>
              <tbody>
                {e.cursos.map((c) => (
                  <tr key={`c${c.id}`}>
                    <td>
                      {c.nombreCurso ?? c.facultad}
                      <span className="tabla__secundario">{c.universidad}</span>
                    </td>
                    <td>{nombreTipoCapacitacion(c.tipoCapacitacion)}</td>
                    <td className="tabla__fecha">{formatoFecha(c.fechaFin ?? c.fechaEvaluacion)}</td>
                    <td className="tabla__num">{formatoNumero(c.horas)}</td>
                  </tr>
                ))}
                {e.equivalencias.map((q) => (
                  <tr key={`e${q.id}`}>
                    <td>{q.descripcion}</td>
                    <td>Equivalencia (CES art. 77)</td>
                    <td className="tabla__fecha">{formatoFecha(q.fecha)}</td>
                    <td className="tabla__num">{q.horasCapacitacion ? formatoNumero(q.horasCapacitacion) : "No suma horas"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DEaDV no tiene cursos de capacitación ni equivalencias para esta cédula.</Vacio>
          ))}

        {activa === "proyectos" &&
          (e.proyectos.length ? (
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Proyecto</th>
                  <th scope="col">Rol</th>
                  <th scope="col">Tipo</th>
                  <th scope="col" className="tabla__num">Meses</th>
                </tr>
              </thead>
              <tbody>
                {e.proyectos.map((p) => (
                  <tr key={p.id}>
                    <td>{p.nombreProyecto}</td>
                    <td>{p.rol}</td>
                    <td>{p.tipoProyecto === "Vinculacion" ? "Vinculación" : "Investigación"}</td>
                    <td className="tabla__num">
                      {p.meses}
                      {p.mesesPonderados !== p.meses && <span className="tabla__secundario">{formatoNumero(p.mesesPonderados)} ponderados</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DIDE no registra proyectos para esta cédula.</Vacio>
          ))}

        {activa === "titulacion" &&
          (e.trabajosTitulacion.length ? (
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Trabajo de titulación</th>
                  <th scope="col">Nivel</th>
                  <th scope="col">Rol</th>
                  <th scope="col">Aprobación</th>
                </tr>
              </thead>
              <tbody>
                {e.trabajosTitulacion.map((t) => (
                  <tr key={t.id}>
                    <td>
                      {t.titulo}
                      <span className="tabla__secundario">{t.universidad}</span>
                    </td>
                    <td>{t.nivel === "Maestria" ? "Maestría" : t.nivel}</td>
                    <td>{t.rol}</td>
                    <td className="tabla__fecha">{formatoFecha(t.fechaAprobacion)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DAC no registra trabajos de titulación dirigidos.</Vacio>
          ))}

        {activa === "investigaciones" &&
          (e.investigaciones.length ? (
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Investigación</th>
                  <th scope="col">Encargado</th>
                  <th scope="col" className="tabla__num">Horas</th>
                  <th scope="col"><span className="sr-only">Documento</span></th>
                </tr>
              </thead>
              <tbody>
                {e.investigaciones.map((inv) => (
                  <tr key={inv.id}>
                    <td>{inv.nombre}</td>
                    <td>{inv.encargado}</td>
                    <td className="tabla__num">{formatoNumero(inv.horas)}</td>
                    <td className="tabla__accion"><BotonPdf base64={inv.pdf} etiqueta={inv.nombre} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <Vacio>DIDE no registra investigaciones para esta cédula.</Vacio>
          ))}

        {activa === "otros" && (
          <dl className="meritos">
            <div>
              <dt>Rol actual desde</dt>
              <dd>{formatoFecha(e.tiempo.fechaInicio)}</dd>
            </div>
            {e.trayectoria.trayectoria && (
              <div>
                <dt>Titular desde</dt>
                <dd>{formatoFecha(e.trayectoria.trayectoria.fechaIngresoTitular)}</dd>
              </div>
            )}
            <div><dt>Meses ponderados en proyectos</dt><dd>{plural(e.mesesPonderados, "mes", "meses")}</dd></div>
            <div><dt>Capacitación impartida</dt><dd>{plural(e.horasImpartidas, "hora")}</dd></div>
            <div><dt>Obra en idioma extranjero</dt><dd>{e.tieneObraEnOtroIdioma ? "Sí" : "No"}</dd></div>
            {e.trayectoria.cargosAutoridad.map((c) => (
              <div key={`cargo${c.id}`}>
                <dt>Cargo de autoridad</dt>
                <dd className="meritos__texto">
                  {c.cargo}: {formatoFecha(c.fechaInicio)} a {c.fechaFin ? formatoFecha(c.fechaFin) : "la fecha"}
                </dd>
              </div>
            ))}
            {e.trayectoria.sabaticos.map((s) => (
              <div key={`sab${s.id}`}>
                <dt>Periodo sabático</dt>
                <dd className="meritos__texto">
                  {formatoFecha(s.fechaInicio)} a {formatoFecha(s.fechaFin)}
                  {s.informeAprobado ? ", informe aprobado" : ", informe pendiente"}
                </dd>
              </div>
            ))}
            <div className="meritos__pdf">
              <dt>Acción de personal ({e.tiempo.rolActual})</dt>
              <dd><BotonPdf base64={e.tiempo.pdfBase64} etiqueta="Acción de personal" /></dd>
            </div>
          </dl>
        )}
      </div>
    </section>
  );
}
