import type { Ruta } from "./ruta";
import type { Sesion } from "./types";

type Seccion = { ruta: Ruta; nombre: string };
type DatosAcceso = Pick<Sesion, "rol" | "esMiembroComision">;

const COMISION: Seccion = { ruta: { vista: "comision" }, nombre: "Comisión" };

// Secciones visibles en la cabecera según el rol y la pertenencia a la Comisión Académica
export function seccionesDe(sesion: DatosAcceso): Seccion[] {
  const conComision = (secciones: Seccion[]) => (sesion.esMiembroComision ? [...secciones, COMISION] : secciones);

  switch (sesion.rol) {
    case "Docente":
      return conComision([
        { ruta: { vista: "expediente" }, nombre: "Mi expediente" },
        { ruta: { vista: "solicitudes" }, nombre: sesion.esMiembroComision ? "Solicitudes" : "Mis solicitudes" },
      ]);
    case "Autoridad":
      // Una autoridad solo consulta expedientes como miembro de la Comisión (la API aplica la misma regla)
      return sesion.esMiembroComision
        ? [COMISION, { ruta: { vista: "solicitudes" }, nombre: "Solicitudes" }, { ruta: { vista: "consulta" }, nombre: "Consultar docente" }]
        : [{ ruta: { vista: "solicitudes" }, nombre: "Solicitudes" }];
    case "RRHH":
      return conComision([
        { ruta: { vista: "solicitudes" }, nombre: "Solicitudes" },
        { ruta: { vista: "consulta" }, nombre: "Consultar docente" },
      ]);
    case "Administrador":
      return [
        { ruta: { vista: "solicitudes" }, nombre: "Solicitudes" },
        { ruta: { vista: "consulta" }, nombre: "Consultar docente" },
        COMISION,
        { ruta: { vista: "usuarios" }, nombre: "Usuarios" },
        { ruta: { vista: "requisitos" }, nombre: "Requisitos" },
        { ruta: { vista: "operacion" }, nombre: "Operación" },
      ];
  }
}

// Qué vistas puede abrir cada usuario (la API vuelve a validar los permisos)
export function rutaPermitida(ruta: Ruta, sesion: DatosAcceso) {
  switch (ruta.vista) {
    case "expediente":
      return sesion.rol === "Docente";
    case "consulta":
      return sesion.rol === "RRHH" || sesion.rol === "Administrador" || sesion.esMiembroComision;
    case "comision":
      return sesion.esMiembroComision || sesion.rol === "Administrador";
    case "usuarios":
    case "requisitos":
    case "operacion":
      return sesion.rol === "Administrador";
    case "solicitudes":
    case "solicitud":
      return true;
    default:
      return false;
  }
}
