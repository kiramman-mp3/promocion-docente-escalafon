import { enlace, type Ruta } from "../ruta";
import { useSesion } from "../sesion";
import type { RolSistema } from "../types";

// Secciones visibles según el rol con el que se inició sesión
const SECCIONES: Record<RolSistema, { ruta: Ruta; nombre: string }[]> = {
  Docente: [
    { ruta: { vista: "expediente" }, nombre: "Mi expediente" },
    { ruta: { vista: "solicitudes" }, nombre: "Mis solicitudes" },
  ],
  RRHH: [
    { ruta: { vista: "solicitudes" }, nombre: "Solicitudes" },
    { ruta: { vista: "consulta" }, nombre: "Consultar docente" },
  ],
  Administrador: [
    { ruta: { vista: "solicitudes" }, nombre: "Solicitudes" },
    { ruta: { vista: "consulta" }, nombre: "Consultar docente" },
    { ruta: { vista: "usuarios" }, nombre: "Usuarios" },
  ],
};

export function seccionesDe(rol: RolSistema) {
  return SECCIONES[rol];
}

export function Cabecera({ ruta }: { ruta: Ruta }) {
  const { sesion, salir } = useSesion();
  const vistaActiva = ruta.vista === "solicitud" ? "solicitudes" : ruta.vista;

  return (
    <header className="cabecera">
      <div className="cabecera__fila">
        <p className="marca">Promoción docente UTA</p>
        <nav className="navegacion" aria-label="Secciones">
          {SECCIONES[sesion.rol].map((s) => (
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
