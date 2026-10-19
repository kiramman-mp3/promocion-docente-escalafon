import { useCallback, useEffect, useState } from "react";
import { obtenerSolicitud } from "../api";
import { Dictamen } from "../components/Dictamen";
import { Escalafon } from "../components/Escalafon";
import { EtiquetaEstado } from "../components/EtiquetaEstado";
import { Evidencia } from "../components/Evidencia";
import { Evidencias } from "../components/solicitud/Evidencias";
import { Resolucion } from "../components/solicitud/Resolucion";
import { RevisionDocumental } from "../components/solicitud/RevisionDocumental";
import { Votacion } from "../components/solicitud/Votacion";
import { formatoFecha } from "../format";
import { enlace } from "../ruta";
import { useSesion } from "../sesion";
import type { Solicitud } from "../types";
import { cumpleTodo, useExpediente } from "../useExpediente";

function titulo(s: Solicitud, esDocente: boolean) {
  switch (s.estado) {
    case "RevisionDocumental":
      return esDocente ? `Tu solicitud para ${s.rolObjetivo} está en revisión documental` : `Revisa los documentos para ${s.rolObjetivo}`;
    case "EnComision":
      return `La Comisión Académica vota la promoción a ${s.rolObjetivo}`;
    case "Aprobada":
      return `Promoción a ${s.rolObjetivo} aprobada`;
    case "Rechazada":
      return `Promoción a ${s.rolObjetivo} negada`;
    case "Apelada":
      return `Apelación en trámite ante el Consejo Universitario`;
    case "Ejecutada":
      return `Promoción a ${s.rolObjetivo} ejecutada`;
  }
}

export function SolicitudDetalle({ id }: { id: number }) {
  const { sesion, roles, mensajeDeError } = useSesion();
  const [solicitud, setSolicitud] = useState<Solicitud | null>(null);
  const [error, setError] = useState<string | null>(null);
  const { expediente } = useExpediente(solicitud?.cedula ?? null);

  const cargar = useCallback(() => {
    obtenerSolicitud(id, sesion.token)
      .then(setSolicitud)
      .catch((e) => setError(mensajeDeError(e)));
  }, [id, sesion.token, mensajeDeError]);

  useEffect(() => {
    setSolicitud(null);
    setError(null);
    cargar();
  }, [cargar]);

  const esDocente = sesion.cedula === solicitud?.cedula;

  return (
    <div className="tablero">
      <aside className="tablero__escalera">
        <Escalafon roles={roles} rolActualId={solicitud?.rolActualId} alcanzaObjetivo={cumpleTodo(solicitud?.dictamen) || solicitud?.estado === "Aprobada"} />
      </aside>

      <main className="tablero__contenido">
        <p className="migas">
          <a href={enlace({ vista: "solicitudes" })}>Solicitudes</a>
        </p>

        {error && (
          <p className="alerta" role="alert">
            {error}
          </p>
        )}
        {!solicitud && !error && <p className="cargando">Cargando solicitud…</p>}

        {solicitud && (
          <div className="resultado">
            <section className="encabezado-solicitud" aria-labelledby="solicitud-titulo">
              <p className="dictamen__contexto">
                Solicitud {solicitud.id} de la cédula {solicitud.cedula}, presentada por {solicitud.solicitadoPor} el{" "}
                {formatoFecha(solicitud.fechaSolicitud)} <EtiquetaEstado estado={solicitud.estado} />
              </p>
              <h1 id="solicitud-titulo" className={`dictamen__veredicto encabezado-solicitud__titulo--${solicitud.estado.toLowerCase()}`}>
                {titulo(solicitud, esDocente)}
              </h1>
              <ol className="linea-tiempo" aria-label="Avance del trámite">
                <li className="hecho">Presentada {formatoFecha(solicitud.fechaSolicitud)}</li>
                <li className={solicitud.fechaEnvioComision ? "hecho" : "actual"}>
                  Revisión documental{solicitud.fechaEnvioComision ? `, enviada a la Comisión el ${formatoFecha(solicitud.fechaEnvioComision)}` : ""}
                </li>
                <li className={solicitud.fechaResolucion ? "hecho" : solicitud.estado === "EnComision" ? "actual" : ""}>
                  Votación de la Comisión{solicitud.fechaResolucion ? `, resuelta el ${formatoFecha(solicitud.fechaResolucion)}` : ""}
                </li>
                <li className={solicitud.estado === "Ejecutada" ? "hecho" : solicitud.estado === "Aprobada" ? "actual" : ""}>
                  Acción de personal{solicitud.accionPersonalNumero ? ` ${solicitud.accionPersonalNumero}` : ""}
                </li>
              </ol>
            </section>

            <Resolucion solicitud={solicitud} onCambio={setSolicitud} />
            {solicitud.votacion && <Votacion solicitud={solicitud} onCambio={setSolicitud} />}
            <Dictamen dictamen={solicitud.dictamen} secundario />
            <Evidencias solicitud={solicitud} onCambio={cargar} />
            {solicitud.documentos && <RevisionDocumental solicitud={solicitud} onCambio={setSolicitud} />}
            {expediente && <Evidencia expediente={expediente} />}
          </div>
        )}
      </main>
    </div>
  );
}
