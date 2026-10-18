// Formas JSON que devuelve ApiREST_UTA (System.Text.Json usa camelCase).

export type RolSistema = "Administrador" | "RRHH" | "Docente";

export interface Sesion {
  token: string;
  id: number;
  nombreCompleto: string;
  correo: string;
  rol: RolSistema;
  cedula: string | null;
}

export interface RolEscalafon {
  id: number;
  nombre: string;
  requiereAniosEnRol: number;
  requiereObras: number;
  requiereEvaluacion: number;
  requiereCapacitacionHoras: number;
  requiereInvestigacionMeses: number;
}

export type TipoDocumento = "TiempoRol" | "Obra" | "Evaluacion" | "Investigacion" | "Curso";
export type EstadoRevision = "Aprobado" | "Rechazado";
export type EstadoSolicitud = "Pendiente" | "Aprobada" | "Rechazada";

export interface RevisionDocumento {
  tipoDocumento: TipoDocumento;
  documentoId: number;
  estado: EstadoRevision;
  observacion: string | null;
  revisadoPor: string | null;
  fechaRevision: string;
}

export interface Solicitud {
  id: number;
  cedula: string;
  rolActualId: number;
  rolActual: string;
  rolObjetivo: string;
  estado: EstadoSolicitud;
  solicitadoPor: string | null;
  fechaSolicitud: string;
  resueltoPor: string | null;
  fechaResolucion: string | null;
  observacion: string | null;
  revisiones: RevisionDocumento[] | null;
}

export interface Usuario {
  id: number;
  correo: string;
  nombre: string;
  apellido: string;
  rol: RolSistema;
  cedula: string | null;
  activo: boolean;
}

export interface TiempoRol {
  id: number;
  cedula: string;
  rolActual: string;
  aniosEnRol: number;
  fechaInicio: string;
  pdfBase64: string;
}

export interface Obra {
  id: number;
  cedula: string;
  fechaPublicacion: string;
  nombreRevista: string;
  pdfBase64: string;
}

export interface Curso {
  id: number;
  cedula: string;
  universidad: string;
  facultad: string;
  fechaEvaluacion: string;
  puntaje: number;
}

export interface Investigacion {
  id: number;
  cedula: string;
  nombre: string;
  encargado: string;
  horas: number;
  pdf: string;
}

export interface Evaluacion {
  id: number;
  cedula: string;
  periodo: string;
  universidad: string;
  carrera: string;
  puntaje: number;
  pdf: string;
}

export interface ResultadoPromocion {
  rolObjetivo: string;
  cumpleExperiencia: boolean;
  cumpleObras: boolean;
  cumpleEvaluacion: boolean;
  cumpleCapacitacion: boolean;
  cumpleInvestigacion: boolean;
  cumpleTesisDoctorado: boolean;
  observaciones: string;
}

export interface Expediente {
  tiempo: TiempoRol;
  rolActualId: number;
  resultado: ResultadoPromocion | null;
  errorResultado: string | null;
  obras: Obra[];
  cursos: Curso[];
  investigaciones: Investigacion[];
  evaluaciones: Evaluacion[];
  tesisDirigidas: number;
  tieneObraEnOtroIdioma: boolean;
  mesesPonderados: number;
  horasImpartidas: number;
}
