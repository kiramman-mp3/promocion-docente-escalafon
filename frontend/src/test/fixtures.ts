import type { Dictamen, Expediente, RolEscalafon } from "../types";

// Catálogo igual al de database/seed/02-rrhh.sql (CES art. 76: Agregado 3 y Principal 3 no tienen promoción)
export const ROLES: RolEscalafon[] = [
  ["Auxiliar", 1],
  ["Auxiliar", 2],
  ["Agregado", 1],
  ["Agregado", 2],
  ["Agregado", 3],
  ["Principal", 1],
  ["Principal", 2],
  ["Principal", 3],
].map(([categoria, nivel], i) => ({
  id: i + 1,
  nombre: `${categoria} ${nivel}`,
  categoria: categoria as string,
  nivel: nivel as number,
  grado: i + 1,
  permanenciaAnios: 4,
  promocionable: i + 1 !== 5 && i + 1 !== 8,
}));

// Dictamen de Ana (Agregado 1) que cumple todo para Agregado 2
export function dictamenAna(cambios: Partial<Dictamen> = {}): Dictamen {
  return {
    cedula: "1801234567",
    fechaCorte: "2026-10-18T00:00:00",
    rolActualId: 3,
    rolActual: "Agregado 1",
    rolObjetivoId: 4,
    rolObjetivo: "Agregado 2",
    promocionable: true,
    requisitosId: 3,
    normativa: "Reglamento UTA 0153-HCU-2026",
    resolucion: "Propuesta DITIC",
    requisitosPropuestos: true,
    cumpleTodo: true,
    requisitos: [
      {
        codigo: "R2",
        nombre: "Permanencia en el grado",
        articulo: "CES art. 76",
        cumple: true,
        exento: false,
        obtenido: "6 años",
        requerido: "4 años",
        detalle: "",
      },
    ],
    alertas: [],
    ...cambios,
  };
}

// Expediente de Ana con su dictamen favorable
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
    dictamen: dictamenAna(),
    errorDictamen: null,
    obras: [],
    cursos: [],
    investigaciones: [],
    evaluaciones: [],
    proyectos: [],
    trabajosTitulacion: [],
    equivalencias: [],
    trayectoria: { trayectoria: null, cargosAutoridad: [], sabaticos: [] },
    tieneObraEnOtroIdioma: true,
    mesesPonderados: 16,
    horasImpartidas: 32,
    ...cambios,
  };
}
