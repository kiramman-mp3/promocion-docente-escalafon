import { createContext, useCallback, useContext } from "react";
import { ApiError } from "./api";
import type { RolEscalafon, Sesion } from "./types";

export interface DatosSesion {
  sesion: Sesion;
  roles: RolEscalafon[];
  salir: () => void;
}

// App.tsx lo provee con <ContextoSesion.Provider> una vez iniciada la sesión
export const ContextoSesion = createContext<DatosSesion | null>(null);

export function useSesion() {
  const ctx = useContext(ContextoSesion);
  if (!ctx) throw new Error("useSesion debe usarse dentro de ProveedorSesion");

  const esRevisor = ctx.sesion.rol === "Administrador" || ctx.sesion.rol === "RRHH";

  // Traduce un error a un mensaje para mostrar; si el token venció, cierra la sesión.
  const mensajeDeError = useCallback(
    (e: unknown) => {
      if (e instanceof ApiError && e.status === 401) {
        ctx.salir();
        return "Tu sesión venció. Vuelve a ingresar.";
      }
      return e instanceof Error ? e.message : "Ocurrió un error inesperado.";
    },
    [ctx],
  );

  return { ...ctx, esRevisor, mensajeDeError };
}
