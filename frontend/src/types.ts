// Formas JSON que devuelve ApiREST_UTA (System.Text.Json usa camelCase).

export interface Sesion {
  token: string;
  nombreCompleto: string;
  correo: string;
  rol: string;
}

export interface TiempoRol {
  cedula: string;
  rolActual: string;
  aniosEnRol: number;
  fechaInicio: string;
  pdfBase64: string;
}

export interface Obra {
  cedula: string;
  fechaPublicacion: string;
  nombreRevista: string;
  pdfBase64: string;
}

export interface Curso {
  cedula: string;
  universidad: string;
  facultad: string;
  fechaEvaluacion: string;
  puntaje: number;
}

export interface Investigacion {
  cedula: string;
  nombre: string;
  encargado: string;
  horas: number;
  pdf: string;
}

export interface Evaluacion {
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
