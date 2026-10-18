import type { EstadoSolicitud } from "../types";

export function EtiquetaEstado({ estado }: { estado: EstadoSolicitud }) {
  return <span className={`etiqueta-estado etiqueta-estado--${estado.toLowerCase()}`}>{estado}</span>;
}
