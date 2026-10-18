// Catálogo del escalafón. Debe coincidir con RRHH.dbo.Roles (database/seed/02-rrhh.sql):
// la API todavía no expone un endpoint de roles y /api/promocion/validar pide el Id del rol actual.
export const ESCALAFON = [
  { id: 1, nombre: "Auxiliar 1" },
  { id: 2, nombre: "Auxiliar 2" },
  { id: 3, nombre: "Agregado 1" },
  { id: 4, nombre: "Agregado 2" },
  { id: 5, nombre: "Agregado 3" },
  { id: 6, nombre: "Principal 1" },
  { id: 7, nombre: "Principal 2" },
  { id: 8, nombre: "Principal 3" },
] as const;

export function idDeRol(nombre: string): number | undefined {
  const buscado = nombre.trim().toLowerCase();
  return ESCALAFON.find((r) => r.nombre.toLowerCase() === buscado)?.id;
}
