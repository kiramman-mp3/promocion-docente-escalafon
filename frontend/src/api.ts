import type {
  CargoComision,
  Curso,
  Dictamen,
  EstadoRevision,
  EstadoSalud,
  EstadoSolicitud,
  Equivalencia,
  Evaluacion,
  EvidenciaArchivo,
  Expediente,
  Investigacion,
  MiembroComision,
  Obra,
  OpcionVoto,
  PaginaAuditoria,
  Proyecto,
  ReporteUso,
  RequisitoPromocion,
  RolEscalafon,
  RolSistema,
  Sesion,
  Solicitud,
  TiempoRol,
  TipoDocumento,
  TrabajoTitulacion,
  Trayectoria,
  Usuario,
} from "./types";

export const API_URL = (import.meta.env.VITE_API_URL ?? "http://localhost:5270").replace(/\/$/, "");

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

async function enviar(metodo: string, ruta: string, token?: string, cuerpo?: unknown): Promise<Response> {
  const esFormulario = cuerpo instanceof FormData;
  let respuesta: Response;
  try {
    respuesta = await fetch(`${API_URL}${ruta}`, {
      method: metodo,
      headers: {
        ...(cuerpo !== undefined && !esFormulario ? { "Content-Type": "application/json" } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: cuerpo === undefined ? undefined : esFormulario ? cuerpo : JSON.stringify(cuerpo),
    });
  } catch {
    throw new ApiError(0, `No se pudo conectar con la API en ${API_URL}. Revisa que esté encendida.`);
  }

  if (!respuesta.ok) {
    let mensaje =
      respuesta.status === 403
        ? "Tu usuario no tiene permiso para esta acción."
        : respuesta.status === 429
          ? "Demasiados intentos. Espera un minuto e intenta de nuevo."
          : `La API respondió ${respuesta.status}.`;
    try {
      const datos = await respuesta.json();
      if (typeof datos?.mensaje === "string") mensaje = datos.mensaje;
    } catch {
      /* respuesta sin JSON */
    }
    throw new ApiError(respuesta.status, mensaje);
  }
  return respuesta;
}

async function pedir<T>(metodo: string, ruta: string, token?: string, cuerpo?: unknown): Promise<T> {
  const respuesta = await enviar(metodo, ruta, token, cuerpo);
  if (respuesta.status === 204) return undefined as T;
  return respuesta.json() as Promise<T>;
}

const post = <T>(ruta: string, cuerpo: unknown, token?: string) => pedir<T>("POST", ruta, token, cuerpo);
const put = <T>(ruta: string, cuerpo: unknown, token: string) => pedir<T>("PUT", ruta, token, cuerpo);
const get = <T>(ruta: string, token?: string) => pedir<T>("GET", ruta, token);

// Compatibilidad: versiones anteriores de la API respondían 404 a las listas vacías
async function lista<T>(ruta: string, cuerpo: unknown, token: string): Promise<T[]> {
  try {
    return await post<T[]>(ruta, cuerpo, token);
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) return [];
    throw e;
  }
}

// ---------- Sesión y catálogos ----------

export function iniciarSesion(correo: string, contrasena: string): Promise<Sesion> {
  return post<Sesion>("/api/Auth/login", { correo, contrasena });
}

export function listarRoles(): Promise<RolEscalafon[]> {
  return get<RolEscalafon[]>("/api/roles");
}

// ---------- Expediente y dictamen ----------

export function obtenerDictamen(cedula: string, token: string): Promise<Dictamen> {
  return post<Dictamen>("/api/promocion/validar", { cedula }, token);
}

export async function cargarExpediente(cedula: string, token: string): Promise<Expediente> {
  const tiempo = await post<TiempoRol>("/api/importar/tiempo-rol", { cedula }, token);

  const dictamen = obtenerDictamen(cedula, token)
    .then((d) => ({ dictamen: d, errorDictamen: null }))
    .catch((e: unknown) => {
      if (e instanceof ApiError && (e.status === 401 || e.status === 403)) throw e;
      return { dictamen: null, errorDictamen: e instanceof Error ? e.message : "No se pudo calcular el dictamen." };
    });

  const [d, obras, cursos, investigaciones, evaluaciones, proyectos, trabajos, equivalencias, trayectoria, idioma, ponderados, impartida] =
    await Promise.all([
      dictamen,
      lista<Obra>("/api/importar/obras", { cedula }, token),
      lista<Curso>("/api/importar/cursos", { cedula }, token),
      lista<Investigacion>("/api/importar/investigaciones", { cedula }, token),
      lista<Evaluacion>("/api/importar/evaluaciones", { cedula }, token),
      lista<Proyecto>("/api/importar/proyectos", { cedula }, token),
      lista<TrabajoTitulacion>("/api/importar/trabajos-titulacion", { cedula }, token),
      lista<Equivalencia>("/api/importar/equivalencias", { cedula }, token),
      post<Trayectoria>("/api/importar/trayectoria", { cedula }, token),
      post<{ tieneObraEnOtroIdioma: boolean }>("/api/importar/obras-idioma", { cedula }, token),
      post<{ mesesPonderados: number }>("/api/importar/proyectos-ponderados", { cedula }, token),
      post<{ horasImpartidas: number }>("/api/importar/capacitacion-impartida", { cedula }, token),
    ]);

  return {
    tiempo,
    ...d,
    obras,
    cursos,
    investigaciones,
    evaluaciones,
    proyectos,
    trabajosTitulacion: trabajos,
    equivalencias,
    trayectoria,
    tieneObraEnOtroIdioma: idioma.tieneObraEnOtroIdioma,
    mesesPonderados: ponderados.mesesPonderados,
    horasImpartidas: impartida.horasImpartidas,
  };
}

// ---------- Solicitudes de promoción ----------

export function crearSolicitud(cedula: string, token: string): Promise<Solicitud> {
  return post<Solicitud>("/api/solicitudes", { cedula }, token);
}

export function listarSolicitudes(token: string, estado?: EstadoSolicitud): Promise<Solicitud[]> {
  return get<Solicitud[]>(`/api/solicitudes${estado ? `?estado=${estado}` : ""}`, token);
}

export function obtenerSolicitud(id: number, token: string): Promise<Solicitud> {
  return get<Solicitud>(`/api/solicitudes/${id}`, token);
}

export function revisarDocumento(
  id: number,
  revision: { tipoDocumento: TipoDocumento; documentoId: number; estado: EstadoRevision; observacion?: string },
  token: string,
): Promise<Solicitud> {
  return put<Solicitud>(`/api/solicitudes/${id}/documentos`, revision, token);
}

export function enviarAComision(id: number, token: string): Promise<Solicitud> {
  return post<Solicitud>(`/api/solicitudes/${id}/enviar-comision`, {}, token);
}

export function votar(
  id: number,
  voto: { voto: OpcionVoto; motivacion: string; certificaPresupuesto?: boolean },
  token: string,
): Promise<Solicitud> {
  return put<Solicitud>(`/api/solicitudes/${id}/votos`, voto, token);
}

export function cerrarVotacion(id: number, motivacion: string, token: string): Promise<Solicitud> {
  return post<Solicitud>(`/api/solicitudes/${id}/cerrar-votacion`, { motivacion }, token);
}

export function registrarAccionPersonal(id: number, numero: string, fecha: string, token: string): Promise<Solicitud> {
  return put<Solicitud>(`/api/solicitudes/${id}/accion-personal`, { numero, fecha }, token);
}

export function apelar(id: number, fundamentos: string, token: string): Promise<Solicitud> {
  return post<Solicitud>(`/api/solicitudes/${id}/apelacion`, { fundamentos }, token);
}

export function resolverApelacion(
  id: number,
  datos: { aceptada: boolean; resolucionHCU: string; motivacion: string },
  token: string,
): Promise<Solicitud> {
  return put<Solicitud>(`/api/solicitudes/${id}/apelacion/resolucion`, datos, token);
}

export function listarEvidencias(id: number, token: string): Promise<EvidenciaArchivo[]> {
  return get<EvidenciaArchivo[]>(`/api/solicitudes/${id}/evidencias`, token);
}

export function subirEvidencia(id: number, archivo: File, tipo: string, descripcion: string, token: string): Promise<EvidenciaArchivo> {
  const datos = new FormData();
  datos.append("archivo", archivo);
  datos.append("tipo", tipo);
  datos.append("descripcion", descripcion);
  return pedir<EvidenciaArchivo>("POST", `/api/solicitudes/${id}/evidencias`, token, datos);
}

// Descarga un PDF protegido (acto o evidencia) y lo abre en otra pestaña
export async function abrirPdfProtegido(ruta: string, token: string) {
  const respuesta = await enviar("GET", ruta, token);
  const url = URL.createObjectURL(await respuesta.blob());
  window.open(url, "_blank", "noopener");
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
}

// ---------- Comisión Académica ----------

export function listarMiembrosComision(token: string, historico = false): Promise<MiembroComision[]> {
  return get<MiembroComision[]>(`/api/comision/miembros${historico ? "?historico=true" : ""}`, token);
}

export function agregarMiembroComision(
  datos: { usuarioId: number; cargo: CargoComision; esPresidente: boolean },
  token: string,
): Promise<MiembroComision> {
  return post<MiembroComision>("/api/comision/miembros", datos, token);
}

export function finalizarMiembroComision(id: number, token: string): Promise<void> {
  return post<void>(`/api/comision/miembros/${id}/finalizar`, {}, token);
}

// ---------- Requisitos de promoción ----------

export function listarRequisitos(historico = false): Promise<RequisitoPromocion[]> {
  return get<RequisitoPromocion[]>(`/api/requisitos${historico ? "?historico=true" : ""}`);
}

export type NuevoRequisito = Omit<RequisitoPromocion, "id" | "rolOrigen" | "gradoDestino" | "rolDestino" | "vigenteHasta" | "vigente">;

export function crearRequisito(datos: NuevoRequisito, token: string): Promise<RequisitoPromocion> {
  return post<RequisitoPromocion>("/api/requisitos", datos, token);
}

// ---------- Operación (Administrador) ----------

export function consultarAuditoria(
  filtro: { desde?: string; hasta?: string; accion?: string; pagina: number },
  token: string,
): Promise<PaginaAuditoria> {
  const parametros = new URLSearchParams({ pagina: String(filtro.pagina), tamanoPagina: "25" });
  if (filtro.desde) parametros.set("desde", filtro.desde);
  if (filtro.hasta) parametros.set("hasta", filtro.hasta);
  if (filtro.accion) parametros.set("accion", filtro.accion);
  return get<PaginaAuditoria>(`/api/auditoria?${parametros}`, token);
}

export function reporteUso(token: string): Promise<ReporteUso> {
  return get<ReporteUso>("/api/reportes/uso", token);
}

// /health responde 503 cuando una base no está disponible, pero el cuerpo trae el detalle
export async function estadoSalud(): Promise<EstadoSalud> {
  try {
    const respuesta = await fetch(`${API_URL}/health`);
    return (await respuesta.json()) as EstadoSalud;
  } catch {
    throw new ApiError(0, `No se pudo conectar con la API en ${API_URL}.`);
  }
}

// ---------- Usuarios (Administrador) ----------

export interface DatosUsuario {
  nombre: string;
  apellido: string;
  rol: RolSistema;
  cedula: string | null;
}

export function listarUsuarios(token: string): Promise<Usuario[]> {
  return get<Usuario[]>("/api/usuarios", token);
}

export function crearUsuario(datos: DatosUsuario & { correo: string; contrasena: string }, token: string) {
  return post<Usuario>("/api/usuarios", datos, token);
}

export function actualizarUsuario(id: number, datos: DatosUsuario & { activo: boolean }, token: string) {
  return put<Usuario>(`/api/usuarios/${id}`, datos, token);
}

export function cambiarContrasena(id: number, contrasena: string, token: string) {
  return put<void>(`/api/usuarios/${id}/contrasena`, { contrasena }, token);
}

// ---------- Utilidades ----------

export function abrirPdf(base64: string) {
  const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
  const url = URL.createObjectURL(new Blob([bytes], { type: "application/pdf" }));
  window.open(url, "_blank", "noopener");
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
}
