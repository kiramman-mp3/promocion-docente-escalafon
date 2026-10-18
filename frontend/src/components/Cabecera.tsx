import { enlace, type Ruta } from "../ruta";
import { seccionesDe } from "../secciones";
import { useSesion } from "../sesion";

export function Cabecera({ ruta }: { ruta: Ruta }) {
  const { sesion, salir } = useSesion();
  const vistaActiva = ruta.vista === "solicitud" ? "solicitudes" : ruta.vista;

  return (
    <header className="cabecera">
      <div className="cabecera__fila">
        <p className="marca">Promoción docente UTA</p>
        <nav className="navegacion" aria-label="Secciones">
          {seccionesDe(sesion.rol).map((s) => (
            <a
              key={s.nombre}
              href={enlace(s.ruta)}
              className="navegacion__enlace"
              aria-current={s.ruta.vista === vistaActiva ? "page" : undefined}
            >
              {s.nombre}
            </a>
          ))}
        </nav>
      </div>
      <div className="cabecera__usuario">
        <span>
          {sesion.nombreCompleto} <span className="cabecera__rol">({sesion.rol})</span>
        </span>
        <button type="button" className="boton-texto" onClick={salir}>
          Cerrar sesión
        </button>
      </div>
    </header>
  );
}
