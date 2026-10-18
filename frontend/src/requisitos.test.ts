import { describe, expect, it } from "vitest";
import { plural } from "./format";
import { requisitosDe } from "./requisitos";
import { expedienteAna } from "./test/fixtures";
import { cumpleTodo } from "./useExpediente";

describe("requisitosDe", () => {
  it("resume cada requisito con los datos del expediente", () => {
    const requisitos = requisitosDe(expedienteAna());

    expect(requisitos).toHaveLength(6);
    expect(requisitos.map((r) => r.detalle)).toEqual([
      expect.stringContaining("6 años"),
      "2 obras desde que asumió el rol",
      "Promedio de 87,5 en 4 periodos (se piden al menos 4)",
      "88 horas en 2 cursos",
      "80 horas en 1 proyecto",
      "No se exige para este rol",
    ]);
  });

  it("indica cuando no hay evaluaciones", () => {
    const [, , evaluacion] = requisitosDe(expedienteAna({ evaluaciones: [] }));
    expect(evaluacion.detalle).toBe("Sin evaluaciones registradas");
  });

  it("cuenta tesis dirigidas cuando el rol objetivo es Principal", () => {
    const requisitos = requisitosDe(expedienteAna({ rolActualId: 6, tesisDirigidas: 2 }));
    const tesis = requisitos[requisitos.length - 1];
    expect(tesis.detalle).toBe("2 tesis registradas en DAC");
  });
});

describe("cumpleTodo", () => {
  it("es verdadero solo si se cumplen todos los requisitos", () => {
    const ana = expedienteAna();
    expect(cumpleTodo(ana)).toBe(true);
    expect(cumpleTodo({ ...ana, resultado: { ...ana.resultado!, cumpleObras: false } })).toBe(false);
    expect(cumpleTodo({ ...ana, resultado: null })).toBe(false);
    expect(cumpleTodo(null)).toBe(false);
  });
});

describe("plural", () => {
  it("usa singular solo para 1", () => {
    expect(plural(1, "obra")).toBe("1 obra");
    expect(plural(0, "obra")).toBe("0 obras");
    expect(plural(3, "mes", "meses")).toBe("3 meses");
  });
});
