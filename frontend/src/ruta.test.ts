import { describe, expect, it } from "vitest";
import { enlace, leerRuta } from "./ruta";

describe("leerRuta", () => {
  it("interpreta las secciones conocidas", () => {
    expect(leerRuta("#/expediente")).toEqual({ vista: "expediente" });
    expect(leerRuta("#/consulta")).toEqual({ vista: "consulta" });
    expect(leerRuta("#/usuarios")).toEqual({ vista: "usuarios" });
    expect(leerRuta("#/solicitudes")).toEqual({ vista: "solicitudes" });
  });

  it("lee el id de una solicitud", () => {
    expect(leerRuta("#/solicitudes/12")).toEqual({ vista: "solicitud", id: 12 });
  });

  it("ignora ids inválidos y rutas desconocidas", () => {
    expect(leerRuta("#/solicitudes/abc")).toEqual({ vista: "solicitudes" });
    expect(leerRuta("#/solicitudes/-3")).toEqual({ vista: "solicitudes" });
    expect(leerRuta("#/otra-cosa")).toEqual({ vista: "inicio" });
    expect(leerRuta("")).toEqual({ vista: "inicio" });
  });
});

describe("enlace", () => {
  it("es el inverso de leerRuta", () => {
    for (const hash of ["#/expediente", "#/consulta", "#/usuarios", "#/solicitudes", "#/solicitudes/7"]) {
      expect(enlace(leerRuta(hash))).toBe(hash);
    }
  });
});
