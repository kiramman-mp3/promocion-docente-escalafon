import { Dictamen } from "../components/Dictamen";
import { Escalafon } from "../components/Escalafon";
import { Evidencia } from "../components/Evidencia";
import { PanelSolicitud } from "../components/PanelSolicitud";
import { useSesion } from "../sesion";
import { cumpleTodo, useExpediente } from "../useExpediente";

// Docente: ve solo su propio expediente (la API rechaza cualquier otra cédula).
export function MiExpediente() {
  const { sesion, roles } = useSesion();
  const { expediente, cargando, error } = useExpediente(sesion.cedula);

  return (
    <div className="tablero">
      <aside className="tablero__escalera">
        <Escalafon roles={roles} rolActualId={expediente?.rolActualId} alcanzaObjetivo={cumpleTodo(expediente)} />
      </aside>

      <main className="tablero__contenido" aria-busy={cargando}>
        {!sesion.cedula && (
          <p className="alerta" role="alert">
            Tu usuario no tiene una cédula asociada. Pide al administrador que la registre para ver tu expediente.
          </p>
        )}
        {cargando && !expediente && <p className="cargando">Cargando tu expediente…</p>}
        {error && (
          <p className="alerta" role="alert">
            {error}
          </p>
        )}
        {expediente && (
          <div className="resultado">
            <Dictamen expediente={expediente} />
            <PanelSolicitud expediente={expediente} />
            <Evidencia expediente={expediente} />
          </div>
        )}
      </main>
    </div>
  );
}
