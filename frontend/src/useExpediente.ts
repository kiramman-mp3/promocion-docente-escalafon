import { useCallback, useEffect, useState } from "react";
import { ApiError, cargarExpediente } from "./api";
import { useSesion } from "./sesion";
import type { Expediente } from "./types";

// Carga el expediente de una cédula; con cedula = null no hace nada.
export function useExpediente(cedula: string | null) {
  const { sesion, roles, mensajeDeError } = useSesion();
  const [expediente, setExpediente] = useState<Expediente | null>(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const cargar = useCallback(
    async (c: string) => {
      setCargando(true);
      setError(null);
      try {
        setExpediente(await cargarExpediente(c, sesion.token, roles));
      } catch (e) {
        setExpediente(null);
        setError(
          e instanceof ApiError && e.status === 404
            ? `RRHH no tiene registrada la cédula ${c}. Revisa el número e intenta de nuevo.`
            : mensajeDeError(e),
        );
      } finally {
        setCargando(false);
      }
    },
    [sesion.token, roles, mensajeDeError],
  );

  useEffect(() => {
    if (cedula && roles.length) cargar(cedula);
  }, [cedula, roles.length, cargar]);

  return { expediente, cargando, error };
}

export function cumpleTodo(expediente: Expediente | null) {
  return (
    !!expediente?.resultado &&
    Object.entries(expediente.resultado).every(([k, v]) => !k.startsWith("cumple") || v === true)
  );
}
