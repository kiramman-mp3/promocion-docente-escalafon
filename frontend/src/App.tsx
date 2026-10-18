import { useEffect, useMemo, useState } from "react";
import { listarRoles } from "./api";
import { Cabecera } from "./components/Cabecera";
import { Login } from "./components/Login";
import { enlace, useRuta } from "./ruta";
import { rutaPermitida, seccionesDe } from "./secciones";
import { ContextoSesion } from "./sesion";
import type { RolEscalafon, Sesion } from "./types";
import { Consulta } from "./vistas/Consulta";
import { MiExpediente } from "./vistas/MiExpediente";
import { SolicitudDetalle } from "./vistas/SolicitudDetalle";
import { Solicitudes } from "./vistas/Solicitudes";
import { Usuarios } from "./vistas/Usuarios";

const CLAVE_SESION = "promocion-docente:sesion";

// La sesión vive en sessionStorage: se cierra al cerrar la pestaña.
function leerSesion(): Sesion | null {
  try {
    const guardada = sessionStorage.getItem(CLAVE_SESION);
    const sesion = guardada ? (JSON.parse(guardada) as Sesion) : null;
    return sesion?.rol ? sesion : null;
  } catch {
    return null;
  }
}

export default function App() {
  const [sesion, setSesion] = useState<Sesion | null>(leerSesion);
  const [roles, setRoles] = useState<RolEscalafon[]>([]);
  const [errorRoles, setErrorRoles] = useState<string | null>(null);
  const ruta = useRuta();

  useEffect(() => {
    listarRoles()
      .then(setRoles)
      .catch((e) => setErrorRoles(e instanceof Error ? e.message : "No se pudo cargar el escalafón."));
  }, []);

  function guardar(nueva: Sesion | null) {
    try {
      if (nueva) sessionStorage.setItem(CLAVE_SESION, JSON.stringify(nueva));
      else sessionStorage.removeItem(CLAVE_SESION);
    } catch {
      /* almacenamiento no disponible: la sesión dura lo que dure la página */
    }
    setSesion(nueva);
    if (!nueva) window.location.hash = "";
  }

  // Cada rol aterriza en su primera sección; rutas no permitidas también vuelven ahí
  useEffect(() => {
    if (sesion && !rutaPermitida(ruta, sesion)) {
      window.location.replace(enlace(seccionesDe(sesion.rol)[0].ruta));
    }
  }, [ruta, sesion]);

  const contexto = useMemo(
    () => (sesion ? { sesion, roles, salir: () => guardar(null) } : null),
    [sesion, roles],
  );

  if (!sesion || !contexto) return <Login roles={roles} errorRoles={errorRoles} onSesion={guardar} />;

  return (
    <ContextoSesion.Provider value={contexto}>
      <div className="app">
        <Cabecera ruta={ruta} />
        {errorRoles && (
          <p className="alerta alerta--pagina" role="alert">
            {errorRoles}
          </p>
        )}
        {rutaPermitida(ruta, sesion) && (
          <>
            {ruta.vista === "expediente" && <MiExpediente />}
            {ruta.vista === "consulta" && <Consulta />}
            {ruta.vista === "solicitudes" && <Solicitudes />}
            {ruta.vista === "solicitud" && <SolicitudDetalle id={ruta.id} />}
            {ruta.vista === "usuarios" && <Usuarios />}
          </>
        )}
      </div>
    </ContextoSesion.Provider>
  );
}
