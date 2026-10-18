import { useEffect, useId, useState } from "react";
import { obtenerSolicitud, resolverSolicitud, revisarDocumento } from "../api";
import { Dictamen } from "../components/Dictamen";
import { Escalafon } from "../components/Escalafon";
import { EtiquetaEstado } from "../components/EtiquetaEstado";
import { Evidencia, type ModoRevision } from "../components/Evidencia";
import { formatoFecha, plural } from "../format";
import { enlace } from "../ruta";
import { useSesion } from "../sesion";
import type { Expediente, Solicitud } from "../types";
import { cumpleTodo, useExpediente } from "../useExpediente";

export function SolicitudDetalle({ id }: { id: number }) {
  const { sesion, roles, esRevisor, mensajeDeError } = useSesion();
  const [solicitud, setSolicitud] = useState<Solicitud | null>(null);
  const [error, setError] = useState<string | null>(null);
  const { expediente, cargando, error: errorExpediente } = useExpediente(solicitud?.cedula ?? null);

  useEffect(() => {
    let vigente = true;
    setSolicitud(null);
    setError(null);
    obtenerSolicitud(id, sesion.token)
      .then((s) => vigente && setSolicitud(s))
      .catch((e) => vigente && setError(mensajeDeError(e)));
    return () => {
      vigente = false;
    };
  }, [id, sesion.token, mensajeDeError]);

  const pendiente = solicitud?.estado === "Pendiente";
  const modoRevision: ModoRevision | undefined = solicitud
    ? {
        revisiones: solicitud.revisiones ?? [],
        editable: esRevisor && pendiente,
        onRevisar: async (tipoDocumento, documentoId, estado, observacion) => {
          try {
            setSolicitud(await revisarDocumento(id, { tipoDocumento, documentoId, estado, observacion }, sesion.token));
          } catch (e) {
            throw new Error(mensajeDeError(e));
          }
        },
      }
    : undefined;

  return (
    <div className="tablero">
      <aside className="tablero__escalera">
        <Escalafon roles={roles} rolActualId={expediente?.rolActualId} alcanzaObjetivo={cumpleTodo(expediente)} />
      </aside>

      <main className="tablero__contenido">
        <p className="migas">
          <a href={enlace({ vista: "solicitudes" })}>{esRevisor ? "Solicitudes" : "Mis solicitudes"}</a>
        </p>

        {error && (
          <p className="alerta" role="alert">
            {error}
          </p>
        )}
        {!solicitud && !error && <p className="cargando">Cargando solicitud…</p>}

        {solicitud && (
          <div className="resultado">
            <EncabezadoSolicitud solicitud={solicitud} esRevisor={esRevisor} />

            {errorExpediente && (
              <p className="alerta" role="alert">
                {errorExpediente}
              </p>
            )}
            {cargando && !expediente && <p className="cargando">Cargando expediente…</p>}

            {expediente && (
              <>
                <Dictamen expediente={expediente} />
                <Evidencia expediente={expediente} modoRevision={modoRevision} />
                {esRevisor && pendiente && (
                  <Resolucion solicitud={solicitud} expediente={expediente} onResuelta={setSolicitud} />
                )}
              </>
            )}
          </div>
        )}
      </main>
    </div>
  );
}

function EncabezadoSolicitud({ solicitud: s, esRevisor }: { solicitud: Solicitud; esRevisor: boolean }) {
  const titulo =
    s.estado === "Aprobada"
      ? `Promoción a ${s.rolObjetivo} aprobada`
      : s.estado === "Rechazada"
        ? `Solicitud para ${s.rolObjetivo} rechazada`
        : esRevisor
          ? `Revisa la solicitud para ${s.rolObjetivo}`
          : `Tu solicitud para ${s.rolObjetivo} está en revisión`;

  return (
    <section className="encabezado-solicitud" aria-labelledby="solicitud-titulo">
      <p className="dictamen__contexto">
        Solicitud {s.id} de la cédula {s.cedula}, registrada por {s.solicitadoPor} el {formatoFecha(s.fechaSolicitud)}{" "}
        <EtiquetaEstado estado={s.estado} />
      </p>
      <h1 id="solicitud-titulo" className={`dictamen__veredicto encabezado-solicitud__titulo--${s.estado.toLowerCase()}`}>
        {titulo}
      </h1>
      {s.estado !== "Pendiente" && (
        <div className={`resolucion-final resolucion-final--${s.estado.toLowerCase()}`}>
          <p>
            Resuelta por {s.resueltoPor} el {s.fechaResolucion && formatoFecha(s.fechaResolucion)}.
          </p>
          {s.observacion && <p className="resolucion-final__observacion">{s.observacion}</p>}
        </div>
      )}
      {s.estado === "Pendiente" && !esRevisor && (
        <p className="encabezado-solicitud__nota">
          RRHH está revisando tus documentos. Aquí verás qué aprobaron y, si rechazan alguno, el motivo.
        </p>
      )}
    </section>
  );
}

function Resolucion({
  solicitud,
  expediente: e,
  onResuelta,
}: {
  solicitud: Solicitud;
  expediente: Expediente;
  onResuelta: (s: Solicitud) => void;
}) {
  const { sesion, mensajeDeError } = useSesion();
  const idObservacion = useId();
  const [observacion, setObservacion] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const total = e.obras.length + e.evaluaciones.length + e.cursos.length + e.investigaciones.length + 1;
  const revisiones = solicitud.revisiones ?? [];
  const rechazados = revisiones.filter((r) => r.estado === "Rechazado").length;
  const faltan = Math.max(total - revisiones.length, 0);

  async function resolver(estado: "Aprobada" | "Rechazada") {
    if (estado === "Rechazada" && !observacion.trim()) {
      setError("Escribe el motivo del rechazo para que el docente sepa qué corregir.");
      return;
    }
    setEnviando(true);
    setError(null);
    try {
      onResuelta(await resolverSolicitud(solicitud.id, estado, observacion.trim(), sesion.token));
      window.scrollTo({ top: 0 });
    } catch (err) {
      setError(mensajeDeError(err));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <section className="resolucion" aria-labelledby="resolucion-titulo">
      <h2 id="resolucion-titulo" className="evidencia__titulo">
        Resolución
      </h2>
      <p className="resolucion__progreso">
        {faltan === 0 ? "Revisaste todos los documentos" : `Faltan ${plural(faltan, "documento")} por revisar`}
        {rechazados > 0 && `, ${rechazados === 1 ? "1 está rechazado" : `${rechazados} están rechazados`}`}.
        {rechazados > 0 && " Para aprobar la promoción no puede quedar ningún documento rechazado."}
      </p>

      <label className="campo" htmlFor={idObservacion}>
        <span className="campo__etiqueta">Observación para el docente (obligatoria si rechazas)</span>
        <textarea
          id={idObservacion}
          rows={3}
          maxLength={1000}
          value={observacion}
          onChange={(ev) => setObservacion(ev.target.value)}
        />
      </label>

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}

      <div className="resolucion__acciones">
        <button
          type="button"
          className="boton boton--aprobar"
          disabled={enviando || rechazados > 0}
          onClick={() => resolver("Aprobada")}
        >
          Aprobar promoción a {solicitud.rolObjetivo}
        </button>
        <button type="button" className="boton boton--secundario" disabled={enviando} onClick={() => resolver("Rechazada")}>
          Rechazar solicitud
        </button>
      </div>
    </section>
  );
}
