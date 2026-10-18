import { createContext, useCallback, useContext, type ReactNode } from "react";
import { ApiError } from "./api";
import type { RolEscalafon, Sesion } from "./types";

interface ContextoSesion {
  sesion: Sesion;
  roles: RolEscalafon[];
  salir: () => void;
}

const Contexto = createContext<ContextoSesion | null>(null);

export function ProveedorSesion({ valor, children }: { valor: ContextoSesion; children: ReactNode }) {
  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useSesion() {
  const ctx = useContext(Contexto);
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
