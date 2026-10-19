import { useEffect, useState } from "react";
import { listarSolicitudes } from "../api";
import { EtiquetaEstado } from "../components/EtiquetaEstado";
import { formatoFecha } from "../format";
import { enlace } from "../ruta";
import { useSesion } from "../sesion";
import type { EstadoSolicitud, Solicitud } from "../types";

const FILTROS: { estado?: EstadoSolicitud; nombre: string }[] = [
  { estado: "RevisionDocumental", nombre: "Revisión documental" },
  { estado: "EnComision", nombre: "En comisión" },
  { estado: "Apelada", nombre: "Apeladas" },
  { estado: "Aprobada", nombre: "Aprobadas" },
  { estado: "Ejecutada", nombre: "Ejecutadas" },
  { estado: "Rechazada", nombre: "Rechazadas" },
  { nombre: "Todas" },
];

export function Solicitudes() {
  const { sesion, esRevisor, mensajeDeError } = useSesion();
  // Talento Humano entra a la revisión documental; la Comisión, a lo que está en votación; el docente ve su historial
  const inicial: EstadoSolicitud | undefined = esRevisor ? "RevisionDocumental" : sesion.rol === "Autoridad" ? "EnComision" : undefined;
  const [filtro, setFiltro] = useState<EstadoSolicitud | undefined>(inicial);
  const [solicitudes, setSolicitudes] = useState<Solicitud[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let vigente = true;
    setSolicitudes(null);
    setError(null);
    listarSolicitudes(sesion.token, filtro)
      .then((s) => vigente && setSolicitudes(s))
      .catch((e) => vigente && setError(mensajeDeError(e)));
    return () => {
      vigente = false;
    };
  }, [filtro, sesion.token, mensajeDeError]);

  const vacio =
    esRevisor || sesion.esMiembroComision || sesion.rol === "Autoridad"
      ? "No hay solicitudes en esta lista."
      : "Todavía no has solicitado una promoción. Hazlo desde Mi expediente.";

  return (
    <main className="pagina">
      <h1 className="pagina__titulo">{esRevisor || sesion.esMiembroComision || sesion.rol === "Autoridad" ? "Solicitudes de promoción" : "Mis solicitudes"}</h1>

      <div className="filtros" role="group" aria-label="Filtrar por estado">
        {FILTROS.map((f) => (
          <button
            key={f.nombre}
            type="button"
            className="filtro"
            aria-pressed={filtro === f.estado}
            onClick={() => setFiltro(f.estado)}
          >
            {f.nombre}
          </button>
        ))}
      </div>

      {error && (
        <p className="alerta" role="alert">
          {error}
        </p>
      )}
      {!solicitudes && !error && <p className="cargando">Cargando solicitudes…</p>}
      {solicitudes?.length === 0 && <p className="evidencia__vacio">{vacio}</p>}

      {!!solicitudes?.length && (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Cédula</th>
                <th scope="col">Promoción</th>
                <th scope="col">Solicitada por</th>
                <th scope="col">Fecha</th>
                <th scope="col">Estado</th>
                <th scope="col"><span className="sr-only">Acción</span></th>
              </tr>
            </thead>
            <tbody>
              {solicitudes.map((s) => (
                <tr key={s.id}>
                  <td className="tabla__cedula">{s.cedula}</td>
                  <td>
                    {s.rolActual} a <strong>{s.rolObjetivo}</strong>
                  </td>
                  <td>{s.solicitadoPor}</td>
                  <td className="tabla__fecha">{formatoFecha(s.fechaSolicitud)}</td>
                  <td><EtiquetaEstado estado={s.estado} /></td>
                  <td className="tabla__accion">
                    <a className="boton-pdf" href={enlace({ vista: "solicitud", id: s.id })}>
                      {(esRevisor && s.estado === "RevisionDocumental") || (sesion.esMiembroComision && s.estado === "EnComision") ? "Revisar" : "Ver"}
                    </a>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </main>
  );
}
