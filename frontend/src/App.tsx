import { useState } from "react";
import { Consulta } from "./components/Consulta";
import { Login } from "./components/Login";
import type { Sesion } from "./types";

const CLAVE_SESION = "promocion-docente:sesion";

// La sesión vive en sessionStorage: se cierra al cerrar la pestaña.
function leerSesion(): Sesion | null {
  try {
    const guardada = sessionStorage.getItem(CLAVE_SESION);
    return guardada ? (JSON.parse(guardada) as Sesion) : null;
  } catch {
    return null;
  }
}

export default function App() {
  const [sesion, setSesion] = useState<Sesion | null>(leerSesion);

  function guardar(nueva: Sesion | null) {
    try {
      if (nueva) sessionStorage.setItem(CLAVE_SESION, JSON.stringify(nueva));
      else sessionStorage.removeItem(CLAVE_SESION);
    } catch {
      /* almacenamiento no disponible: la sesión dura lo que dure la página */
    }
    setSesion(nueva);
  }

  return sesion ? <Consulta sesion={sesion} onSalir={() => guardar(null)} /> : <Login onSesion={guardar} />;
}
