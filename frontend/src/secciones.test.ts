import { describe, expect, it } from "vitest";
import { rutaPermitida, seccionesDe } from "./secciones";
import type { RolSistema } from "./types";

const acceso = (rol: RolSistema, esMiembroComision = false) => ({ rol, esMiembroComision });

describe("rutaPermitida", () => {
  it("el docente solo ve su expediente y sus solicitudes", () => {
    const docente = acceso("Docente");
    expect(rutaPermitida({ vista: "expediente" }, docente)).toBe(true);
    expect(rutaPermitida({ vista: "solicitudes" }, docente)).toBe(true);
    expect(rutaPermitida({ vista: "solicitud", id: 1 }, docente)).toBe(true);
    expect(rutaPermitida({ vista: "consulta" }, docente)).toBe(false);
    expect(rutaPermitida({ vista: "comision" }, docente)).toBe(false);
    expect(rutaPermitida({ vista: "usuarios" }, docente)).toBe(false);
  });

  it("el docente miembro de la Comisión consulta expedientes y vota", () => {
    const miembro = acceso("Docente", true);
    expect(rutaPermitida({ vista: "comision" }, miembro)).toBe(true);
    expect(rutaPermitida({ vista: "consulta" }, miembro)).toBe(true);
    expect(rutaPermitida({ vista: "requisitos" }, miembro)).toBe(false);
  });

  it("RRHH consulta docentes pero no administra usuarios ni requisitos", () => {
    const rrhh = acceso("RRHH");
    expect(rutaPermitida({ vista: "consulta" }, rrhh)).toBe(true);
    expect(rutaPermitida({ vista: "expediente" }, rrhh)).toBe(false);
    expect(rutaPermitida({ vista: "usuarios" }, rrhh)).toBe(false);
    expect(rutaPermitida({ vista: "requisitos" }, rrhh)).toBe(false);
  });

  it("el administrador gestiona usuarios, requisitos y operación", () => {
    const admin = acceso("Administrador");
    expect(rutaPermitida({ vista: "usuarios" }, admin)).toBe(true);
    expect(rutaPermitida({ vista: "requisitos" }, admin)).toBe(true);
    expect(rutaPermitida({ vista: "operacion" }, admin)).toBe(true);
    expect(rutaPermitida({ vista: "comision" }, admin)).toBe(true);
  });

  it("la ruta de inicio nunca se muestra: redirige a la primera sección", () => {
    expect(rutaPermitida({ vista: "inicio" }, acceso("Administrador"))).toBe(false);
  });
});

describe("seccionesDe", () => {
  it("cada usuario aterriza en una sección que tiene permitida", () => {
    for (const rol of ["Docente", "RRHH", "Autoridad", "Administrador"] as const) {
      for (const miembro of [false, true]) {
        const [primera] = seccionesDe(acceso(rol, miembro));
        expect(rutaPermitida(primera.ruta, acceso(rol, miembro))).toBe(true);
      }
    }
  });

  it("toda sección visible está permitida", () => {
    for (const rol of ["Docente", "RRHH", "Autoridad", "Administrador"] as const) {
      for (const s of seccionesDe(acceso(rol, true))) expect(rutaPermitida(s.ruta, acceso(rol, true))).toBe(true);
    }
  });

  it("solo el administrador tiene usuarios y requisitos", () => {
    expect(seccionesDe(acceso("Administrador")).map((s) => s.nombre)).toEqual(expect.arrayContaining(["Usuarios", "Requisitos"]));
    expect(seccionesDe(acceso("RRHH")).map((s) => s.nombre)).not.toContain("Usuarios");
  });

  it("la Comisión aparece solo para sus miembros", () => {
    expect(seccionesDe(acceso("Docente")).map((s) => s.nombre)).not.toContain("Comisión");
    expect(seccionesDe(acceso("Docente", true)).map((s) => s.nombre)).toContain("Comisión");
  });
});
