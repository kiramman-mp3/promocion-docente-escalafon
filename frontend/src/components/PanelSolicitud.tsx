import { useEffect, useState } from "react";
import { crearSolicitud, listarSolicitudes } from "../api";
import { formatoFecha } from "../format";
import { enlace, navegar } from "../ruta";
import { useSesion } from "../sesion";
import type { Expediente, Solicitud } from "../types";
import { EtiquetaEstado } from "./EtiquetaEstado";

// Debajo del dictamen: muestra la última solicitud del docente o permite crear una nueva.
export function PanelSolicitud({ expediente }: { expediente: Expediente }) {
  const { sesion, esRevisor, mensajeDeError } = useSesion();
  const cedula = expediente.tiempo.cedula;
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

  if (!expediente.resultado || ultima === undefined) return null;
  const objetivo = expediente.resultado.rolObjetivo;

  return (
    <section className="panel-solicitud" aria-label="Solicitud de promoción">
      {ultima?.estado === "Pendiente" ? (
        <p>
          {esRevisor ? "Este docente ya tiene" : "Ya tienes"} una solicitud para {ultima.rolObjetivo} en revisión desde
          el {formatoFecha(ultima.fechaSolicitud)}. <a href={enlace({ vista: "solicitud", id: ultima.id })}>Ver solicitud</a>
        </p>
      ) : ultima?.estado === "Aprobada" && ultima.rolObjetivo === objetivo ? (
        <p>
          La promoción a {objetivo} fue aprobada el {ultima.fechaResolucion && formatoFecha(ultima.fechaResolucion)}. El
          expediente mostrará el nuevo rol cuando RRHH lo registre.{" "}
          <a href={enlace({ vista: "solicitud", id: ultima.id })}>Ver solicitud</a>
        </p>
      ) : (
        <>
          <div>
            <p className="panel-solicitud__titulo">
              {esRevisor ? `Registrar la solicitud de promoción a ${objetivo}` : `Solicitar mi promoción a ${objetivo}`}
            </p>
            <p className="panel-solicitud__texto">
              RRHH revisará cada documento del expediente antes de aprobarla.
              {ultima && (
                <>
                  {" "}
                  La solicitud anterior quedó <EtiquetaEstado estado={ultima.estado} />.
                </>
              )}
            </p>
          </div>
          <button type="button" className="boton" onClick={solicitar} disabled={enviando}>
            {enviando ? "Enviando…" : esRevisor ? "Registrar solicitud" : "Solicitar promoción"}
          </button>
        </>
      )}
      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
    </section>
  );
}
