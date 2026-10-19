import { nombreEstado } from "../format";
import type { EstadoSolicitud } from "../types";

export function EtiquetaEstado({ estado }: { estado: EstadoSolicitud }) {
  return <span className={`etiqueta-estado etiqueta-estado--${estado.toLowerCase()}`}>{nombreEstado(estado)}</span>;
}
