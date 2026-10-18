import type {
  Curso,
  EstadoRevision,
  EstadoSolicitud,
  Evaluacion,
  Expediente,
  Investigacion,
  Obra,
  ResultadoPromocion,
  RolEscalafon,
  RolSistema,
  Sesion,
  Solicitud,
  TiempoRol,
  TipoDocumento,
  Usuario,
} from "./types";

const API_URL = (import.meta.env.VITE_API_URL ?? "http://localhost:5270").replace(/\/$/, "");

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

async function pedir<T>(metodo: string, ruta: string, token?: string, cuerpo?: unknown): Promise<T> {
  let respuesta: Response;
  try {
    respuesta = await fetch(`${API_URL}${ruta}`, {
      method: metodo,
      headers: {
        ...(cuerpo !== undefined ? { "Content-Type": "application/json" } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: cuerpo !== undefined ? JSON.stringify(cuerpo) : undefined,
    });
  } catch {
    throw new ApiError(0, `No se pudo conectar con la API en ${API_URL}. Revisa que esté encendida.`);
  }

  if (!respuesta.ok) {
    let mensaje =
      respuesta.status === 403
        ? "Tu usuario no tiene permiso para esta acción."
        : `La API respondió ${respuesta.status}.`;
    try {
      const datos = await respuesta.json();
      if (typeof datos?.mensaje === "string") mensaje = datos.mensaje;
    } catch {
      /* respuesta sin JSON */
    }
    throw new ApiError(respuesta.status, mensaje);
  }
  if (respuesta.status === 204) return undefined as T;
  return respuesta.json() as Promise<T>;
}

const post = <T>(ruta: string, cuerpo: unknown, token?: string) => pedir<T>("POST", ruta, token, cuerpo);
const put = <T>(ruta: string, cuerpo: unknown, token: string) => pedir<T>("PUT", ruta, token, cuerpo);
const get = <T>(ruta: string, token: string) => pedir<T>("GET", ruta, token);

// Los endpoints de listas responden 404 cuando no hay registros: para el expediente eso es una lista vacía.
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
  return pedir<RolEscalafon[]>("GET", "/api/roles");
}

// ---------- Expediente ----------

export async function cargarExpediente(cedula: string, token: string, roles: RolEscalafon[]): Promise<Expediente> {
  const tiempo = await post<TiempoRol>("/api/importar/tiempo-rol", { cedula }, token);
  const rolActual = roles.find((r) => r.nombre.toLowerCase() === tiempo.rolActual.trim().toLowerCase());
  if (!rolActual) {
    throw new ApiError(422, `El rol "${tiempo.rolActual}" registrado en RRHH no está en el escalafón.`);
  }
  const esUltimoRol = rolActual.id === Math.max(...roles.map((r) => r.id));

  const validar = esUltimoRol
    ? Promise.resolve({ resultado: null, errorResultado: `${rolActual.nombre} es el último rol del escalafón.` })
    : post<ResultadoPromocion>("/api/promocion/validar", { cedula, rolActualId: rolActual.id }, token)
        .then((resultado) => ({ resultado, errorResultado: null }))
        .catch((e: unknown) => {
          if (e instanceof ApiError && (e.status === 401 || e.status === 403)) throw e;
          return { resultado: null, errorResultado: "La API no pudo calcular el dictamen para este docente." };
        });

  const [dictamen, obras, cursos, investigaciones, evaluaciones, tesis, idioma, proyectos, impartida] =
    await Promise.all([
      validar,
      lista<Obra>("/api/importar/obras", { cedula, fechaInicio: tiempo.fechaInicio }, token),
      lista<Curso>("/api/importar/cursos", { cedula }, token),
      lista<Investigacion>("/api/importar/investigaciones", { cedula }, token),
      lista<Evaluacion>("/api/importar/evaluaciones", { cedula }, token),
      post<{ cantidad: number }>("/api/importar/tesis-doctorado", { cedula }, token),
      post<{ tieneObraEnOtroIdioma: boolean }>("/api/importar/obras-idioma", { cedula }, token),
      post<{ mesesPonderados: number }>("/api/importar/proyectos-ponderados", { cedula }, token),
      post<{ horasImpartidas: number }>("/api/importar/capacitacion-impartida", { cedula }, token),
    ]);

  return {
    tiempo,
    rolActualId: rolActual.id,
    ...dictamen,
    obras,
    cursos,
    investigaciones,
    evaluaciones,
    tesisDirigidas: tesis.cantidad,
    tieneObraEnOtroIdioma: idioma.tieneObraEnOtroIdioma,
    mesesPonderados: proyectos.mesesPonderados,
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

export function resolverSolicitud(
  id: number,
  estado: "Aprobada" | "Rechazada",
  observacion: string,
  token: string,
): Promise<Solicitud> {
  return put<Solicitud>(`/api/solicitudes/${id}/resolucion`, { estado, observacion }, token);
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
