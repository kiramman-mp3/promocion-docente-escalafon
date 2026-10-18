import { useEffect, useState } from "react";
import { crearSolicitud, listarSolicitudes } from "../api";
import { EN_TRAMITE, formatoFecha, nombreEstado } from "../format";
import { enlace, navegar } from "../ruta";
import { useSesion } from "../sesion";
import type { Expediente, Solicitud } from "../types";
import { EtiquetaEstado } from "./EtiquetaEstado";

// Debajo del dictamen: muestra la última solicitud del docente o permite presentar una nueva (CES art. 99)
export function PanelSolicitud({ expediente }: { expediente: Expediente }) {
  const { sesion, esRevisor, mensajeDeError } = useSesion();
  const cedula = expediente.tiempo.cedula;
  const dictamen = expediente.dictamen;
  const [ultima, setUltima] = useState<Solicitud | null | undefined>(undefined);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let vigente = true;
    setUltima(undefined);
    listarSolicitudes(sesion.token)
      .then((todas) => vigente && setUltima(todas.find((s) => s.cedula === cedula) ?? null))
      .catch((e) => vigente && (setUltima(null), setError(mensajeDeError(e))));
    return () => {
      vigente = false;
    };
  }, [cedula, sesion.token, mensajeDeError]);

  async function solicitar() {
    setEnviando(true);
    setError(null);
    try {
      const nueva = await crearSolicitud(cedula, sesion.token);
      navegar({ vista: "solicitud", id: nueva.id });
    } catch (e) {
      setError(mensajeDeError(e));
      setEnviando(false);
    }
  }

  const esPropia = sesion.cedula === cedula;
  if (!dictamen || ultima === undefined || (!esPropia && !esRevisor)) return null;
  if (!dictamen.promocionable) return null;

  const objetivo = dictamen.rolObjetivo;
  const permanencia = dictamen.requisitos.find((r) => r.codigo === "R2");
  const enTramite = ultima && EN_TRAMITE.includes(ultima.estado);
  const yaAprobada = ultima && (ultima.estado === "Aprobada" || ultima.estado === "Ejecutada") && ultima.rolObjetivo === objetivo;

  let contenido;
  if (enTramite) {
    contenido = (
      <p>
        {esPropia ? "Tu solicitud" : "La solicitud"} para {ultima.rolObjetivo} está {nombreEstado(ultima.estado).toLowerCase()} desde el{" "}
        {formatoFecha(ultima.fechaSolicitud)}. <a href={enlace({ vista: "solicitud", id: ultima.id })}>Ver solicitud</a>
      </p>
    );
  } else if (yaAprobada) {
    contenido = (
      <p>
        La promoción a {objetivo} fue aprobada{ultima.fechaResolucion ? ` el ${formatoFecha(ultima.fechaResolucion)}` : ""}. El expediente
        mostrará el nuevo rol cuando RRHH lo registre. <a href={enlace({ vista: "solicitud", id: ultima.id })}>Ver solicitud</a>
      </p>
    );
  } else if (permanencia && !permanencia.cumple) {
    contenido = (
      <div>
        <p className="panel-solicitud__titulo">Aún no puede solicitar la promoción a {objetivo}</p>
        <p className="panel-solicitud__texto">
          La solicitud se presenta al cumplir la permanencia mínima en el grado (CES art. 99): tiene {permanencia.obtenido} y se exigen{" "}
          {permanencia.requerido}.
        </p>
      </div>
    );
  } else {
    contenido = (
      <>
        <div>
          <p className="panel-solicitud__titulo">
            {esPropia ? `Solicitar mi promoción a ${objetivo}` : `Registrar la solicitud de promoción a ${objetivo}`}
          </p>
          <p className="panel-solicitud__texto">
            Talento Humano revisará cada documento y la Comisión Académica votará la solicitud.
            {!dictamen.cumpleTodo && " Puedes presentarla aunque falten requisitos; la Comisión decide con el expediente completo."}
            {ultima && (
              <>
                {" "}
                La solicitud anterior quedó <EtiquetaEstado estado={ultima.estado} />.
              </>
            )}
          </p>
        </div>
        <button type="button" className="boton" onClick={solicitar} disabled={enviando}>
          {enviando ? "Enviando…" : esPropia ? "Solicitar promoción" : "Registrar solicitud"}
        </button>
      </>
    );
  }

  return (
    <section className="panel-solicitud" aria-label="Solicitud de promoción">
      {contenido}
      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
    </section>
  );
}
