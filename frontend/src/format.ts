import type { EstadoSolicitud } from "./types";

const fechaLarga = new Intl.DateTimeFormat("es-EC", { day: "numeric", month: "long", year: "numeric" });
const fechaMes = new Intl.DateTimeFormat("es-EC", { month: "short", year: "numeric" });
const fechaHora = new Intl.DateTimeFormat("es-EC", { dateStyle: "medium", timeStyle: "short" });
const numero = new Intl.NumberFormat("es-EC", { maximumFractionDigits: 2 });

export const formatoFecha = (iso: string) => fechaLarga.format(new Date(iso));
export const formatoMes = (iso: string) => fechaMes.format(new Date(iso));
export const formatoFechaHora = (iso: string) => fechaHora.format(new Date(iso));
export const formatoNumero = (n: number) => numero.format(n);

export function plural(n: number, singular: string, pluralTexto = `${singular}s`) {
  return `${formatoNumero(n)} ${n === 1 ? singular : pluralTexto}`;
}

const ESTADOS: Record<EstadoSolicitud, string> = {
  RevisionDocumental: "En revisión documental",
  EnComision: "En comisión",
  Aprobada: "Aprobada",
  Rechazada: "Rechazada",
  Apelada: "Apelada",
  Ejecutada: "Ejecutada",
};

export const nombreEstado = (estado: EstadoSolicitud) => ESTADOS[estado] ?? estado;

export const EN_TRAMITE: EstadoSolicitud[] = ["RevisionDocumental", "EnComision", "Apelada"];

const CARGOS: Record<string, string> = {
  VicerrectorAcademico: "Vicerrector Académico",
  ProfesorHCU: "Profesor designado por el HCU",
  ProfesorTerna: "Profesor de terna",
  DirectorFinanciero: "Director Financiero",
  DirectorTalentoHumano: "Director de Talento Humano",
};

export const nombreCargo = (cargo: string | null | undefined) => (cargo ? (CARGOS[cargo] ?? cargo) : "");

const TIPOS_OBRA: Record<string, string> = {
  ArticuloArbitrado: "Artículo arbitrado",
  Libro: "Libro",
  CapituloLibro: "Capítulo de libro",
  Ponencia: "Ponencia",
  PropiedadIndustrial: "Propiedad industrial",
  ProduccionArtistica: "Producción artística",
  DisenoSoftwarePrototipo: "Software o prototipo",
  Otra: "Otra",
};

export const nombreTipoObra = (tipo: string) => TIPOS_OBRA[tipo] ?? tipo;

const TIPOS_CAPACITACION: Record<string, string> = {
  Pedagogica: "Pedagógica",
  Cientifica: "Científica",
  CongresoInternacional: "Congreso internacional",
};

export const nombreTipoCapacitacion = (tipo: string) => TIPOS_CAPACITACION[tipo] ?? tipo;

const TIPOS_DOCUMENTO: Record<string, string> = {
  TiempoRol: "Acción de personal",
  Obra: "Obras",
  Evaluacion: "Evaluaciones",
  Curso: "Capacitación",
  Investigacion: "Investigaciones",
  Proyecto: "Proyectos",
  TrabajoTitulacion: "Trabajos de titulación",
  Equivalencia: "Equivalencias",
  Evidencia: "Evidencias del docente",
};

export const nombreTipoDocumento = (tipo: string) => TIPOS_DOCUMENTO[tipo] ?? tipo;
