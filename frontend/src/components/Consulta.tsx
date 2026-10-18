import { useState, type FormEvent } from "react";
import { ApiError, cargarExpediente } from "../api";
import type { Expediente, Sesion } from "../types";
import { Dictamen } from "./Dictamen";
import { Escalafon } from "./Escalafon";
import { Evidencia } from "./Evidencia";

interface Props {
  sesion: Sesion;
  onSalir: () => void;
}

const CEDULAS_DE_PRUEBA = ["1801234567", "1809876543", "1712345678"];

export function Consulta({ sesion, onSalir }: Props) {
  const [cedula, setCedula] = useState("");
  const [expediente, setExpediente] = useState<Expediente | null>(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const cedulaValida = /^\d{10}$/.test(cedula);

  async function consultar(ev?: FormEvent, cedulaElegida = cedula) {
    ev?.preventDefault();
    if (!/^\d{10}$/.test(cedulaElegida)) return;
    setCargando(true);
    setError(null);
    try {
      setExpediente(await cargarExpediente(cedulaElegida, sesion.token));
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) return onSalir();
      setExpediente(null);
      setError(
        e instanceof ApiError && e.status === 404
          ? `RRHH no tiene registrada la cédula ${cedulaElegida}. Revisa el número e intenta de nuevo.`
          : e instanceof Error
            ? e.message
            : "No se pudo cargar el expediente.",
      );
    } finally {
      setCargando(false);
    }
  }

  const cumpleTodo =
    !!expediente?.resultado &&
    Object.entries(expediente.resultado).every(([k, v]) => !k.startsWith("cumple") || v === true);

  return (
    <div className="app">
      <header className="cabecera">
        <p className="marca">Promoción docente UTA</p>
        <div className="cabecera__usuario">
          <span>
            {sesion.nombreCompleto} <span className="cabecera__rol">({sesion.rol})</span>
          </span>
          <button type="button" className="boton-texto" onClick={onSalir}>
            Cerrar sesión
          </button>
        </div>
      </header>

      <div className="tablero">
        <aside className="tablero__escalera">
          <Escalafon rolActualId={expediente?.rolActualId} alcanzaObjetivo={cumpleTodo} />
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
                aria-describedby="cedula-ayuda"
              />
            </label>
            <button className="boton" type="submit" disabled={!cedulaValida || cargando}>
              {cargando ? "Consultando…" : "Consultar expediente"}
            </button>
            {import.meta.env.DEV && (
              <p id="cedula-ayuda" className="busqueda__ayuda">
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
                <Dictamen expediente={expediente} />
                <Evidencia expediente={expediente} />
              </div>
            )}
          </div>
        </main>
      </div>
    </div>
  );
}
