import { describe, expect, it } from "vitest";
import { rutaPermitida, seccionesDe } from "./secciones";

describe("rutaPermitida", () => {
  it("el docente solo ve su expediente y sus solicitudes", () => {
    const docente = { rol: "Docente" } as const;
    expect(rutaPermitida({ vista: "expediente" }, docente)).toBe(true);
    expect(rutaPermitida({ vista: "solicitudes" }, docente)).toBe(true);
    expect(rutaPermitida({ vista: "solicitud", id: 1 }, docente)).toBe(true);
    expect(rutaPermitida({ vista: "consulta" }, docente)).toBe(false);
    expect(rutaPermitida({ vista: "usuarios" }, docente)).toBe(false);
  });

  it("RRHH consulta docentes pero no administra usuarios", () => {
    const rrhh = { rol: "RRHH" } as const;
    expect(rutaPermitida({ vista: "consulta" }, rrhh)).toBe(true);
    expect(rutaPermitida({ vista: "expediente" }, rrhh)).toBe(false);
    expect(rutaPermitida({ vista: "usuarios" }, rrhh)).toBe(false);
  });

  it("el administrador puede gestionar usuarios", () => {
    expect(rutaPermitida({ vista: "usuarios" }, { rol: "Administrador" })).toBe(true);
  });

  it("la ruta de inicio nunca se muestra: redirige a la primera sección", () => {
    expect(rutaPermitida({ vista: "inicio" }, { rol: "Administrador" })).toBe(false);
  });
});

describe("seccionesDe", () => {
  it("cada rol aterriza en una sección que tiene permitida", () => {
    for (const rol of ["Docente", "RRHH", "Administrador"] as const) {
      const [primera] = seccionesDe(rol);
      expect(rutaPermitida(primera.ruta, { rol })).toBe(true);
    }
  });

  it("solo el administrador tiene la sección de usuarios", () => {
    expect(seccionesDe("Administrador").map((s) => s.nombre)).toContain("Usuarios");
    expect(seccionesDe("RRHH").map((s) => s.nombre)).not.toContain("Usuarios");
  });
});
