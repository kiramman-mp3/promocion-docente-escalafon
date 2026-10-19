// Formas JSON que devuelve ApiREST_UTA (System.Text.Json usa camelCase).

export type RolSistema = "Administrador" | "RRHH" | "Autoridad" | "Docente";

export interface Sesion {
  token: string;
  id: number;
  nombreCompleto: string;
  correo: string;
  rol: RolSistema;
  cedula: string | null;
  esMiembroComision: boolean;
  esPresidenteComision: boolean;
  cargoComision: string | null;
}

// Grado del escalafón (CES art. 63)
export interface RolEscalafon {
  id: number;
  nombre: string;
  categoria: string | null;
  nivel: number | null;
  grado: number | null;
  permanenciaAnios: number | null;
  promocionable: boolean;
}

// ---------- Dictamen de promoción ----------

export interface ResultadoRequisito {
  codigo: string;
  nombre: string;
  articulo: string;
  cumple: boolean;
  exento: boolean;
  obtenido: string;
  requerido: string;
  detalle: string;
}

export interface Dictamen {
  cedula: string;
  fechaCorte: string;
  rolActualId: number;
  rolActual: string;
  rolObjetivoId: number | null;
  rolObjetivo: string | null;
  promocionable: boolean;
  requisitosId: number | null;
  normativa: string | null;
  resolucion: string | null;
  requisitosPropuestos: boolean;
  cumpleTodo: boolean;
  requisitos: ResultadoRequisito[];
  alertas: string[];
}

// ---------- Expediente ----------

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
  titulo: string | null;
  fechaPublicacion: string;
  nombreRevista: string;
  idioma: string;
  tipoObra: string;
  issn: string | null;
  isbn: string | null;
  baseIndexacion: string | null;
  revisionPares: boolean;
  registroSENADI: string | null;
  esRelevante: boolean;
  motivoNoRelevante: string | null;
  pdfBase64: string;
}

export interface Curso {
  id: number;
  cedula: string;
  nombreCurso: string | null;
  universidad: string;
  facultad: string;
  fechaEvaluacion: string;
  puntaje: number;
  horas: number;
  tipoCapacitacion: "Pedagogica" | "Cientifica" | "CongresoInternacional";
  fechaInicio: string | null;
  fechaFin: string | null;
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
  periodoAcademico: string | null;
  universidad: string;
  carrera: string;
  puntaje: number;
  ejercioDocencia: boolean;
  pdf: string;
}

export interface Proyecto {
  id: number;
  cedula: string;
  nombreProyecto: string | null;
  rol: "Director" | "Codirector" | "Participante";
  tipoProyecto: "Investigacion" | "Vinculacion";
  meses: number;
  mesesPonderados: number;
  fechaInicio: string | null;
  fechaFin: string | null;
}

export interface TrabajoTitulacion {
  id: number;
  cedula: string;
  titulo: string;
  nivel: "Grado" | "Maestria" | "Doctorado";
  rol: "Director" | "Codirector";
  fechaAprobacion: string;
  universidad: string;
}

export interface Equivalencia {
  id: number;
  cedula: string;
  tipo: string;
  fecha: string;
  meses: number | null;
  descripcion: string;
  horasCapacitacion: number;
}

export interface Trayectoria {
  trayectoria: {
    fechaIngresoInstitucion: string;
    fechaIngresoTitular: string;
    mesesNoTitular: number;
    mesesTecnicoDocente: number;
    ingresoConPhD: boolean;
  } | null;
  cargosAutoridad: { id: number; cargo: string; tipoCargo: string; fechaInicio: string; fechaFin: string | null }[];
  sabaticos: { id: number; fechaInicio: string; fechaFin: string; informeAprobado: boolean }[];
}

export interface Expediente {
  tiempo: TiempoRol;
  dictamen: Dictamen | null;
  errorDictamen: string | null;
  obras: Obra[];
  cursos: Curso[];
  investigaciones: Investigacion[];
  evaluaciones: Evaluacion[];
  proyectos: Proyecto[];
  trabajosTitulacion: TrabajoTitulacion[];
  equivalencias: Equivalencia[];
  trayectoria: Trayectoria;
  tieneObraEnOtroIdioma: boolean;
  mesesPonderados: number;
  horasImpartidas: number;
}

// ---------- Solicitudes ----------

export type TipoDocumento =
  | "TiempoRol"
  | "Obra"
  | "Evaluacion"
  | "Investigacion"
  | "Curso"
  | "Proyecto"
  | "TrabajoTitulacion"
  | "Equivalencia"
  | "Evidencia";
export type EstadoRevision = "Aprobado" | "Rechazado";
export type EstadoSolicitud = "RevisionDocumental" | "EnComision" | "Aprobada" | "Rechazada" | "Apelada" | "Ejecutada";
export type OpcionVoto = "AFavor" | "EnContra" | "Abstencion";

export interface RevisionDocumento {
  tipoDocumento: TipoDocumento;
  documentoId: number;
  estado: EstadoRevision;
  observacion: string | null;
  revisadoPor: string | null;
  fechaRevision: string;
}

export interface DocumentoSolicitud {
  tipo: TipoDocumento;
  id: number;
  descripcion: string;
  revision: RevisionDocumento | null;
}

export interface Voto {
  miembro: string | null;
  cargo: string | null;
  esPresidente: boolean;
  voto: OpcionVoto;
  motivacion: string;
  certificaPresupuesto: boolean | null;
  fecha: string;
}

export interface Votacion {
  miembrosActivos: number;
  quorum: number;
  votosEmitidos: number;
  aFavor: number;
  enContra: number;
  abstenciones: number;
  presupuestoCertificado: boolean;
  esMiembro: boolean;
  esPresidente: boolean;
  miCargo: string | null;
  puedeVotar: boolean;
  motivoNoPuedeVotar: string | null;
  puedeCerrar: boolean;
  miVoto: Voto | null;
}

export interface ApelacionInfo {
  fundamentos: string;
  fechaPresentacion: string;
  estado: "Pendiente" | "Aceptada" | "Negada";
  resolucionHCU: string | null;
  motivacion: string | null;
  fechaResolucion: string | null;
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
  requisitosPromocionId: number | null;
  fechaEnvioComision: string | null;
  numeroResolucion: string | null;
  resueltoPor: string | null;
  fechaResolucion: string | null;
  observacion: string | null;
  actoDisponible: boolean;
  actoHash: string | null;
  accionPersonalNumero: string | null;
  accionPersonalFecha: string | null;
  fechaEjecucion: string | null;
  dictamen: Dictamen | null;
  revisiones: RevisionDocumento[] | null;
  documentos: DocumentoSolicitud[] | null;
  votos: Voto[] | null;
  votacion: Votacion | null;
  apelacion: ApelacionInfo | null;
  plazoApelacionHasta: string | null;
  puedeApelar: boolean;
}

export interface EvidenciaArchivo {
  id: number;
  solicitudId: number;
  tipo: string;
  descripcion: string;
  nombreArchivo: string;
  hash: string;
  tamano: number;
  subidoPor: string | null;
  fechaSubida: string;
}

// ---------- Administración ----------

export interface Usuario {
  id: number;
  correo: string;
  nombre: string;
  apellido: string;
  rol: RolSistema;
  cedula: string | null;
  activo: boolean;
}

export type CargoComision = "VicerrectorAcademico" | "ProfesorHCU" | "ProfesorTerna" | "DirectorFinanciero" | "DirectorTalentoHumano";

export interface MiembroComision {
  id: number;
  usuarioId: number;
  nombre: string | null;
  correo: string | null;
  cedula: string | null;
  cargo: CargoComision;
  esPresidente: boolean;
  vigenteDesde: string;
  vigenteHasta: string | null;
}

export interface RequisitoPromocion {
  id: number;
  gradoOrigen: number;
  rolOrigen: string;
  gradoDestino: number;
  rolDestino: string;
  normativa: string;
  resolucion: string;
  esPropuesta: boolean;
  vigenteDesde: string;
  vigenteHasta: string | null;
  vigente: boolean;
  obrasTotales: number;
  obrasEnGrado: number;
  evaluacionMinima: number;
  evaluacionPeriodos: number;
  capacitacionHoras: number;
  capacitacionAnios: number;
  capacitacionPctPedagogica: number;
  proyectosMeses: number;
  requiereDireccionProyecto: boolean;
  trabajosTitulacion: number;
  titulacionNivelMinimo: "Grado" | "Maestria" | "Doctorado";
}

export interface RegistroAuditoria {
  id: number;
  fecha: string;
  usuarioId: number | null;
  usuario: string | null;
  accion: string;
  metodo: string;
  ruta: string;
  estado: number;
  ip: string | null;
  detalle: string | null;
}

export interface PaginaAuditoria {
  total: number;
  pagina: number;
  tamanoPagina: number;
  registros: RegistroAuditoria[];
}

export interface ReporteUso {
  generadoEn: string;
  solicitudesPorEstado: Record<string, number>;
  diasPromedioHastaResolucion: number | null;
  solicitudesPorMes: { mes: string; cantidad: number }[];
  usuariosActivos: number;
  usuariosDesactivados: number;
  ingresosExitosos30Dias: number;
  ingresosFallidos30Dias: number;
  operacionesEscritura30Dias: number;
  erroresServidor30Dias: number;
}

export interface EstadoSalud {
  estado: string;
  duracionMs: number;
  componentes: Record<string, { estado: string; descripcion: string | null; bases: Record<string, string> }>;
}
