import { idDeRol } from "./roles";
import type {
  Curso,
  Evaluacion,
  Expediente,
  Investigacion,
  Obra,
  ResultadoPromocion,
  Sesion,
  TiempoRol,
} from "./types";

const API_URL = (import.meta.env.VITE_API_URL ?? "http://localhost:5270").replace(/\/$/, "");

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

async function post<T>(ruta: string, cuerpo: unknown, token?: string): Promise<T> {
  let respuesta: Response;
  try {
    respuesta = await fetch(`${API_URL}${ruta}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: JSON.stringify(cuerpo),
    });
  } catch {
    throw new ApiError(0, `No se pudo conectar con la API en ${API_URL}. Revisa que esté encendida.`);
  }

  if (!respuesta.ok) {
    let mensaje = `La API respondió ${respuesta.status}.`;
    try {
      const datos = await respuesta.json();
      if (typeof datos?.mensaje === "string") mensaje = datos.mensaje;
    } catch {
      /* respuesta sin JSON */
    }
    throw new ApiError(respuesta.status, mensaje);
  }
  return respuesta.json() as Promise<T>;
}

// Los endpoints de listas responden 404 cuando no hay registros: para el expediente eso es una lista vacía.
async function lista<T>(ruta: string, cuerpo: unknown, token: string): Promise<T[]> {
  try {
    return await post<T[]>(ruta, cuerpo, token);
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) return [];
    throw e;
  }
}

export function iniciarSesion(correo: string, contrasena: string): Promise<Sesion> {
  return post<Sesion>("/api/Auth/login", { correo, contrasena });
}

export async function cargarExpediente(cedula: string, token: string): Promise<Expediente> {
  const tiempo = await post<TiempoRol>("/api/importar/tiempo-rol", { cedula }, token);
  const rolActualId = idDeRol(tiempo.rolActual);
  if (!rolActualId) {
    throw new ApiError(422, `El rol "${tiempo.rolActual}" registrado en RRHH no está en el escalafón.`);
  }

  const validar = post<ResultadoPromocion>("/api/promocion/validar", { cedula, rolActualId }, token)
    .then((resultado) => ({ resultado, errorResultado: null }))
    .catch((e: unknown) => {
      if (e instanceof ApiError && e.status === 401) throw e;
      return {
        resultado: null,
        errorResultado:
          rolActualId === 8
            ? "Este docente ya está en Principal 3, el último rol del escalafón."
            : "La API no pudo calcular el dictamen para este docente.",
      };
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
    rolActualId,
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

export function abrirPdf(base64: string) {
  const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
  const url = URL.createObjectURL(new Blob([bytes], { type: "application/pdf" }));
  window.open(url, "_blank", "noopener");
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
}
