import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError, cargarExpediente, iniciarSesion, listarSolicitudes } from "./api";
import { dictamenAna } from "./test/fixtures";

type Respuesta = { status: number; cuerpo?: unknown };

// Simula fetch respondiendo según la ruta pedida
function simularApi(respuestas: Record<string, Respuesta>) {
  const llamadas: string[] = [];
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const ruta = url.replace(/^https?:\/\/[^/]+/, "");
      llamadas.push(ruta);
      const r = respuestas[ruta] ?? { status: 404, cuerpo: { mensaje: "No encontrado" } };
      return new Response(r.cuerpo === undefined ? null : JSON.stringify(r.cuerpo), { status: r.status });
    }),
  );
  return llamadas;
}

const tiempoRol = (rolActual: string) => ({
  status: 200,
  cuerpo: { id: 1, cedula: "1801234567", rolActual, aniosEnRol: 6, fechaInicio: "2020-03-01", pdfBase64: "" },
});

const contadores = {
  "/api/importar/trayectoria": { status: 200, cuerpo: { trayectoria: null, cargosAutoridad: [], sabaticos: [] } },
  "/api/importar/obras-idioma": { status: 200, cuerpo: { tieneObraEnOtroIdioma: false } },
  "/api/importar/proyectos-ponderados": { status: 200, cuerpo: { mesesPonderados: 3 } },
  "/api/importar/capacitacion-impartida": { status: 200, cuerpo: { horasImpartidas: 0 } },
};

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("pedidos a la API", () => {
  it("usa el mensaje de error que devuelve la API", async () => {
    simularApi({ "/api/Auth/login": { status: 401, cuerpo: { mensaje: "Credenciales inválidas" } } });

    await expect(iniciarSesion("x@uta.edu.ec", "mal")).rejects.toMatchObject({
      status: 401,
      message: "Credenciales inválidas",
    });
  });

  it("explica un 403 sin cuerpo como falta de permisos", async () => {
    simularApi({ "/api/solicitudes": { status: 403 } });

    await expect(listarSolicitudes("token")).rejects.toThrow("no tiene permiso");
  });

  it("filtra solicitudes por estado en la URL", async () => {
    const llamadas = simularApi({ "/api/solicitudes?estado=RevisionDocumental": { status: 200, cuerpo: [] } });

    await listarSolicitudes("token", "RevisionDocumental");

    expect(llamadas).toEqual(["/api/solicitudes?estado=RevisionDocumental"]);
  });
});

describe("cargarExpediente", () => {
  it("trata los 404 de las listas como listas vacías", async () => {
    simularApi({
      "/api/importar/tiempo-rol": tiempoRol("Agregado 1"),
      "/api/promocion/validar": { status: 200, cuerpo: dictamenAna({ cumpleTodo: false }) },
      ...contadores,
    });

    const expediente = await cargarExpediente("1801234567", "token");

    expect(expediente.obras).toEqual([]);
    expect(expediente.evaluaciones).toEqual([]);
    expect(expediente.proyectos).toEqual([]);
    expect(expediente.mesesPonderados).toBe(3);
    expect(expediente.dictamen?.rolObjetivo).toBe("Agregado 2");
    expect(expediente.errorDictamen).toBeNull();
  });

  it("muestra el expediente aunque el dictamen no se pueda calcular", async () => {
    simularApi({
      "/api/importar/tiempo-rol": tiempoRol("Agregado 1"),
      "/api/promocion/validar": { status: 422, cuerpo: { mensaje: "No hay requisitos vigentes para Agregado 1." } },
      ...contadores,
    });

    const expediente = await cargarExpediente("1801234567", "token");

    expect(expediente.dictamen).toBeNull();
    expect(expediente.errorDictamen).toBe("No hay requisitos vigentes para Agregado 1.");
  });

  it("propaga el 403 cuando un docente pide una cédula ajena", async () => {
    simularApi({ "/api/importar/tiempo-rol": { status: 403, cuerpo: { mensaje: "Solo puedes consultar tu propio expediente." } } });

    await expect(cargarExpediente("1712345678", "token")).rejects.toMatchObject({ status: 403 });
  });

  it("no oculta un 401 del dictamen como error de cálculo", async () => {
    simularApi({
      "/api/importar/tiempo-rol": tiempoRol("Agregado 1"),
      "/api/promocion/validar": { status: 401 },
      ...contadores,
    });

    const error = await cargarExpediente("1801234567", "token").catch((e) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(401);
  });
});
