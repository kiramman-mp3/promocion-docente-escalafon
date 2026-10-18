import type { Expediente, RolEscalafon } from "../types";

// Catálogo igual al de database/seed/02-rrhh.sql
export const ROLES: RolEscalafon[] = [
  "Auxiliar 1",
  "Auxiliar 2",
  "Agregado 1",
  "Agregado 2",
  "Agregado 3",
  "Principal 1",
  "Principal 2",
  "Principal 3",
].map((nombre, i) => ({
  id: i + 1,
  nombre,
  requiereAniosEnRol: 4,
  requiereObras: 0,
  requiereEvaluacion: 75,
  requiereCapacitacionHoras: 0,
  requiereInvestigacionMeses: 0,
}));

// Expediente de Ana (Agregado 1) que cumple todo para Agregado 2
export function expedienteAna(cambios: Partial<Expediente> = {}): Expediente {
  return {
    tiempo: {
      id: 1,
      cedula: "1801234567",
      rolActual: "Agregado 1",
      aniosEnRol: 6,
      fechaInicio: "2020-03-01T00:00:00",
      pdfBase64: "JVBERi0=",
    },
    rolActualId: 3,
    resultado: {
      rolObjetivo: "Agregado 2",
      cumpleExperiencia: true,
      cumpleObras: true,
      cumpleEvaluacion: true,
      cumpleCapacitacion: true,
      cumpleInvestigacion: true,
      cumpleTesisDoctorado: true,
      observaciones: "",
    },
    errorResultado: null,
    obras: [
      { id: 1, cedula: "1801234567", fechaPublicacion: "2021-06-22T00:00:00", nombreRevista: "IEEE", pdfBase64: "" },
      { id: 2, cedula: "1801234567", fechaPublicacion: "2022-09-05T00:00:00", nombreRevista: "Ingenius", pdfBase64: "" },
    ],
    cursos: [
      { id: 1, cedula: "1801234567", universidad: "UTA", facultad: "FISEI", fechaEvaluacion: "2021-02-10T00:00:00", puntaje: 40 },
      { id: 2, cedula: "1801234567", universidad: "UTA", facultad: "FCHE", fechaEvaluacion: "2022-07-18T00:00:00", puntaje: 48 },
    ],
    investigaciones: [{ id: 1, cedula: "1801234567", nombre: "Alerta temprana", encargado: "Ana", horas: 80, pdf: "" }],
    evaluaciones: [80, 90, 85, 95].map((puntaje, i) => ({
      id: i + 1,
      cedula: "1801234567",
      periodo: `202${i}-03-01T00:00:00`,
      universidad: "UTA",
      carrera: "Software",
      puntaje,
      pdf: "",
    })),
    tesisDirigidas: 0,
    tieneObraEnOtroIdioma: true,
    mesesPonderados: 16,
    horasImpartidas: 32,
    ...cambios,
  };
}
