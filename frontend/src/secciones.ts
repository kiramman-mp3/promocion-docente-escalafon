import type { Ruta } from "./ruta";
import type { RolSistema, Sesion } from "./types";

// Secciones visibles en la cabecera según el rol con el que se inició sesión
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

// Qué vistas puede abrir cada rol (la API vuelve a validar los permisos)
export function rutaPermitida(ruta: Ruta, sesion: Pick<Sesion, "rol">) {
  switch (ruta.vista) {
    case "expediente":
      return sesion.rol === "Docente";
    case "consulta":
      return sesion.rol !== "Docente";
    case "usuarios":
      return sesion.rol === "Administrador";
    case "solicitudes":
    case "solicitud":
      return true;
    default:
      return false;
  }
}
