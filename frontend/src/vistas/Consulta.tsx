import { useState, type FormEvent } from "react";
import { Dictamen } from "../components/Dictamen";
import { Escalafon } from "../components/Escalafon";
import { Evidencia } from "../components/Evidencia";
import { PanelSolicitud } from "../components/PanelSolicitud";
import { useSesion } from "../sesion";
import { cumpleTodo, useExpediente } from "../useExpediente";

const CEDULAS_DE_PRUEBA = ["1801234567", "1809876543", "1712345678", "1805555555", "1701111111"];

// RRHH y Administrador: buscan cualquier docente por cédula.
export function Consulta() {
  const { roles } = useSesion();
  const [cedula, setCedula] = useState("");
  const [consultada, setConsultada] = useState<string | null>(null);
  const { expediente, cargando, error } = useExpediente(consultada);

  function consultar(ev?: FormEvent, c = cedula) {
    ev?.preventDefault();
    if (/^\d{10}$/.test(c)) setConsultada(c);
  }

  return (
    <div className="tablero">
      <aside className="tablero__escalera">
        <Escalafon roles={roles} rolActualId={expediente?.dictamen?.rolActualId} alcanzaObjetivo={cumpleTodo(expediente?.dictamen)} />
      </aside>

      <main className="tablero__contenido">
        <form className="busqueda" onSubmit={consultar} role="search">
          <label className="campo busqueda__campo">
            <span className="campo__etiqueta">Cédula del docente</span>
            <input
              inputMode="numeric"
              autoComplete="off"
              maxLength={10}
              value={cedula}
              onChange={(e) => setCedula(e.target.value.replace(/\D/g, ""))}
              placeholder="10 dígitos"
            />
          </label>
          <button className="boton" type="submit" disabled={!/^\d{10}$/.test(cedula) || cargando}>
            {cargando ? "Consultando…" : "Consultar expediente"}
          </button>
          {import.meta.env.DEV && (
            <p className="busqueda__ayuda">
              Docentes de prueba:{" "}
              {CEDULAS_DE_PRUEBA.map((c, i) => (
                <span key={c}>
                  {i > 0 && ", "}
                  <button
                    type="button"
                    className="boton-texto"
                    onClick={() => {
                      setCedula(c);
                      consultar(undefined, c);
                    }}
                  >
                    {c}
                  </button>
                </span>
              ))}
            </p>
          )}
        </form>

        <div aria-live="polite" aria-busy={cargando}>
          {error && (
            <p className="alerta" role="alert">
              {error}
            </p>
          )}

          {!expediente && !error && !cargando && (
            <div className="inicio">
              <h1 className="inicio__titulo">¿Quién quiere subir de rol?</h1>
              <p>
                Escribe la cédula del docente. Traeremos su tiempo en el rol, obras, evaluaciones, cursos e
                investigaciones, y te diremos qué requisitos cumple para el siguiente rol del escalafón.
              </p>
            </div>
          )}

          {expediente && (
            <div className={cargando ? "resultado resultado--cargando" : "resultado"}>
              <Dictamen dictamen={expediente.dictamen} error={expediente.errorDictamen} />
              <PanelSolicitud expediente={expediente} />
              <Evidencia expediente={expediente} />
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
